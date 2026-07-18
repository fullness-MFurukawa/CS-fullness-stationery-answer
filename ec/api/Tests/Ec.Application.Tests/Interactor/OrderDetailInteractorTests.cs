using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Application.Params;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class OrderDetailInteractorTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private OrderDetailInteractor _interactor = null!;

    private static Customer CreateCustomer()
        => new(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", null,
            "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.UtcNow);

    private static Order CreateOrder(Customer customer)
    {
        var product = new Product(
            Guid.NewGuid(), "ボールペン", 150, null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));
        return Order.Create(
            Guid.NewGuid(),
            customer,
            new OrderStatus(OrderStatus.OrderedId, "注文済"),
            new PaymentMethod(1, "現金"),
            [new OrderDetail(product, 1)],
            DateTime.UtcNow);
    }

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _interactor = new OrderDetailInteractor(_orderRepository.Object);
    }

    [TestMethod(DisplayName = "自分の注文を取得できる")]
    public async Task ExecuteAsync_OwnOrder_ReturnsOrder()
    {
        var customer = CreateCustomer();
        var order = CreateOrder(customer);
        _orderRepository
            .Setup(r => r.FindByIdAsync(order.Id, customer.Id))
            .ReturnsAsync(order);

        var result = await _interactor.ExecuteAsync(new OrderDetailParam(order.Id, customer.Id));

        Assert.AreEqual(order.Id, result.Id);
    }

    [TestMethod(DisplayName = "注文が存在しない、または他人の注文の場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_NotFoundOrOthersOrder_ThrowsNotFoundException()
    {
        // リポジトリが注文IDと顧客IDの両方で絞り込むため、
        // 他人の注文はnullが返る。存在しない場合と区別しない
        _orderRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new OrderDetailParam(Guid.NewGuid(), Guid.NewGuid())));
    }

    [TestMethod(DisplayName = "リポジトリには注文IDと顧客IDの両方を渡す")]
    public async Task ExecuteAsync_PassesBothIdsToRepository()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        _orderRepository
            .Setup(r => r.FindByIdAsync(orderId, customerId))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new OrderDetailParam(orderId, customerId)));

        // 顧客IDによる絞り込みが確実に行われることを検証する。
        // これが抜けると他人の注文を閲覧できてしまう
        _orderRepository.Verify(r => r.FindByIdAsync(orderId, customerId), Times.Once);
    }
}