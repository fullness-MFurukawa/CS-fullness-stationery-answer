using Ec.Application.Interactor;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class OrderHistorySearchInteractorTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private OrderHistorySearchInteractor _interactor = null!;

    /// <summary>
    /// テスト用の注文を生成する
    /// </summary>
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

    private static Customer CreateCustomer()
        => new(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", null,
            "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.UtcNow);

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _interactor = new OrderHistorySearchInteractor(_orderRepository.Object);
    }

    [TestMethod(DisplayName = "顧客の購入履歴を返す")]
    public async Task ExecuteAsync_ReturnsCustomerOrders()
    {
        var customer = CreateCustomer();
        var orders = new List<Order> { CreateOrder(customer), CreateOrder(customer) };
        _orderRepository
            .Setup(r => r.FindByCustomerAsync(customer.Id))
            .ReturnsAsync(orders);

        var result = await _interactor.ExecuteAsync(customer.Id);

        Assert.HasCount(2, result);
        _orderRepository.Verify(r => r.FindByCustomerAsync(customer.Id), Times.Once);
    }

    [TestMethod(DisplayName = "購入履歴が0件なら空の一覧を返す")]
    public async Task ExecuteAsync_NoOrders_ReturnsEmpty()
    {
        _orderRepository
            .Setup(r => r.FindByCustomerAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var result = await _interactor.ExecuteAsync(Guid.NewGuid());

        Assert.IsEmpty(result);
    }
}