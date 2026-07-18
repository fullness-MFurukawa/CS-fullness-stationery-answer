using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Application.Params;
using Ec.Application.Tests.Fakes;
using Ec.Domain.Exceptions;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class OrderCreateInteractorTests
{
    private Mock<ICustomerRepository> _customerRepository = null!;
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IPaymentMethodRepository> _paymentMethodRepository = null!;
    private Mock<IOrderStatusRepository> _orderStatusRepository = null!;
    private Mock<IOrderRepository> _orderRepository = null!;
    private OrderCreateInteractor _interactor = null!;

    private Customer _customer = null!;
    private PaymentMethod _paymentMethod = null!;
    private OrderStatus _orderedStatus = null!;
    private Product _productA = null!;
    private Product _productB = null!;

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(Guid id, string name, int price, int stock)
        => new(
            id,
            name,
            price,
            null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), stock));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _customer = new Customer(
            Guid.NewGuid(),
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            null,
            "090-1234-5678",
            "test@example.com",
            "testuser",
            "hashed-password",
            DateTime.UtcNow);

        _paymentMethod = new PaymentMethod(1, "現金");
        _orderedStatus = new OrderStatus(OrderStatus.OrderedId, "注文済");
        _productA = CreateProduct(Guid.NewGuid(), "ボールペン", 150, 10);
        _productB = CreateProduct(Guid.NewGuid(), "ノート", 200, 5);

        _customerRepository = new Mock<ICustomerRepository>();
        _customerRepository
            .Setup(r => r.FindByIdAsync(_customer.Id))
            .ReturnsAsync(_customer);

        _paymentMethodRepository = new Mock<IPaymentMethodRepository>();
        _paymentMethodRepository
            .Setup(r => r.FindByIdAsync(_paymentMethod.Id))
            .ReturnsAsync(_paymentMethod);

        _orderStatusRepository = new Mock<IOrderStatusRepository>();
        _orderStatusRepository
            .Setup(r => r.FindByIdAsync(OrderStatus.OrderedId))
            .ReturnsAsync(_orderedStatus);

        _productRepository = new Mock<IProductRepository>();
        _productRepository
            .Setup(r => r.FindByIdsForUpdateAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([_productA, _productB]);
        _productRepository
            .Setup(r => r.UpdateStockAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _orderRepository = new Mock<IOrderRepository>();
        _orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _interactor = new OrderCreateInteractor(
            _customerRepository.Object,
            _productRepository.Object,
            _paymentMethodRepository.Object,
            _orderStatusRepository.Object,
            _orderRepository.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private OrderCreateParam CreateParam(params OrderItemParam[] items)
        => new(_customer.Id, _paymentMethod.Id, items);

    [TestMethod(DisplayName = "注文を確定し合計金額が算出される")]
    public async Task ExecuteAsync_ValidParam_CreatesOrder()
    {
        var param = CreateParam(
            new OrderItemParam(_productA.Id, 2),   // 150 × 2 = 300
            new OrderItemParam(_productB.Id, 1));  // 200 × 1 = 200

        var order = await _interactor.ExecuteAsync(param);

        Assert.AreEqual(_customer, order.Customer);
        Assert.AreEqual(_paymentMethod, order.PaymentMethod);
        Assert.AreEqual(OrderStatus.OrderedId, order.Status.Id);
        Assert.HasCount(2, order.Details);
        Assert.AreEqual(500, order.AmountTotal);
        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    [TestMethod(DisplayName = "注文した数量分だけ在庫を減らして保存する")]
    public async Task ExecuteAsync_ValidParam_ReducesStock()
    {
        var updated = new List<Product>();
        _productRepository
            .Setup(r => r.UpdateStockAsync(It.IsAny<Product>()))
            .Callback<Product>(p => updated.Add(p))
            .Returns(Task.CompletedTask);

        await _interactor.ExecuteAsync(CreateParam(
            new OrderItemParam(_productA.Id, 3),
            new OrderItemParam(_productB.Id, 2)));

        // 商品Aは10→7、商品Bは5→3
        var savedA = updated.Single(p => p.Id == _productA.Id);
        var savedB = updated.Single(p => p.Id == _productB.Id);
        Assert.AreEqual(7, savedA.Stock.Quantity);
        Assert.AreEqual(3, savedB.Stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫が不足する場合はDomainExceptionをスローし注文を登録しない")]
    public async Task ExecuteAsync_InsufficientStock_ThrowsAndDoesNotAddOrder()
    {
        // 商品Bの在庫は5。6を注文する
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(
                new OrderItemParam(_productB.Id, 6))));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [TestMethod(DisplayName = "在庫ちょうどの数量は購入できる")]
    public async Task ExecuteAsync_ExactStock_Succeeds()
    {
        // 商品Bの在庫は5。5を注文する（境界値）
        var order = await _interactor.ExecuteAsync(CreateParam(
            new OrderItemParam(_productB.Id, 5)));

        Assert.AreEqual(1000, order.AmountTotal);
    }

    [TestMethod(DisplayName = "注文する商品が空ならDomainExceptionをスローする")]
    public async Task ExecuteAsync_EmptyItems_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "同じ商品が複数指定されている場合はDomainExceptionをスローする")]
    public async Task ExecuteAsync_DuplicatedProduct_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(
                new OrderItemParam(_productA.Id, 1),
                new OrderItemParam(_productA.Id, 2))));

        // ロックを取る前に弾くため、在庫の取得も行わない
        _productRepository.Verify(
            r => r.FindByIdsForUpdateAsync(It.IsAny<IReadOnlyCollection<Guid>>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "指定した商品が取得できない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // ロックで取得できるのは商品Aのみ（商品Bは削除された想定）
        _productRepository
            .Setup(r => r.FindByIdsForUpdateAsync(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync([_productA]);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam(
                new OrderItemParam(_productA.Id, 1),
                new OrderItemParam(_productB.Id, 1))));

        _orderRepository.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
    }

    [TestMethod(DisplayName = "顧客が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_CustomerNotFound_ThrowsNotFoundException()
    {
        _customerRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Customer?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam(
                new OrderItemParam(_productA.Id, 1))));
    }

    [TestMethod(DisplayName = "支払い方法が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_PaymentMethodNotFound_ThrowsNotFoundException()
    {
        _paymentMethodRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PaymentMethod?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam(
                new OrderItemParam(_productA.Id, 1))));
    }
}