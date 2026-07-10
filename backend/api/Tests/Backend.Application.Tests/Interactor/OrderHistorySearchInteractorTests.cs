using Backend.Application.Interactor;
using Backend.Application.Params;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class OrderHistorySearchInteractorTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private OrderHistorySearchInteractor _interactor = null!;

    /// <summary>
    /// テスト用の注文を生成する
    /// </summary>
    private static Order CreateOrder(int amountTotal = 240)
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var product = new Product(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, null, category,
            new ProductStock(Guid.NewGuid(), 10));
        var detail = new OrderDetail(1, product, 2);
        var customer = new Customer(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", "テストビル101",
            "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.Now);

        return new Order(
            Guid.NewGuid(),
            new DateTime(2024, 5, 12, 15, 30, 0),
            amountTotal,
            customer,
            new OrderStatus(3, "配送中"),
            new PaymentMethod(1, "現金"),
            [detail]);
    }

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _orderRepository = new Mock<IOrderRepository>();
        _interactor = new OrderHistorySearchInteractor(_orderRepository.Object);
    }

    [TestMethod(DisplayName = "条件を指定しない場合はすべての注文を取得する")]
    public async Task ExecuteAsync_WithoutConditions_SearchesWithNullConditions()
    {
        IReadOnlyList<Order> expected = [CreateOrder(), CreateOrder()];
        _orderRepository.Setup(r => r.SearchAsync(null, null)).ReturnsAsync(expected);

        var orders = await _interactor.ExecuteAsync(new OrderHistorySearchParam(null, null));

        Assert.HasCount(2, orders);
        _orderRepository.Verify(r => r.SearchAsync(null, null), Times.Once);
    }

    [TestMethod(DisplayName = "購入日を指定した場合はその条件で検索する")]
    public async Task ExecuteAsync_WithOrderDate_PassesOrderDate()
    {
        var orderDate = new DateOnly(2024, 5, 12);
        IReadOnlyList<Order> expected = [CreateOrder(100)];
        _orderRepository.Setup(r => r.SearchAsync(orderDate, null)).ReturnsAsync(expected);

        var orders = await _interactor.ExecuteAsync(new OrderHistorySearchParam(orderDate, null));

        Assert.HasCount(1, orders);
        _orderRepository.Verify(r => r.SearchAsync(orderDate, null), Times.Once);
    }

    [TestMethod(DisplayName = "顧客アカウント名を指定した場合はその条件で検索する")]
    public async Task ExecuteAsync_WithCustomerAccountName_PassesAccountName()
    {
        IReadOnlyList<Order> expected = [CreateOrder(3800)];
        _orderRepository.Setup(r => r.SearchAsync(null, "taro123")).ReturnsAsync(expected);

        var orders = await _interactor.ExecuteAsync(new OrderHistorySearchParam(null, "taro123"));

        Assert.HasCount(1, orders);
        _orderRepository.Verify(r => r.SearchAsync(null, "taro123"), Times.Once);
    }

    [TestMethod(DisplayName = "購入日と顧客アカウント名の両方を指定した場合は両方の条件で検索する")]
    public async Task ExecuteAsync_WithBothConditions_PassesBothConditions()
    {
        var orderDate = new DateOnly(2024, 5, 12);
        IReadOnlyList<Order> expected = [CreateOrder(100)];
        _orderRepository.Setup(r => r.SearchAsync(orderDate, "testuser")).ReturnsAsync(expected);

        var orders = await _interactor.ExecuteAsync(new OrderHistorySearchParam(orderDate, "testuser"));

        Assert.HasCount(1, orders);
        _orderRepository.Verify(r => r.SearchAsync(orderDate, "testuser"), Times.Once);
    }

    [TestMethod(DisplayName = "該当する注文が0件でも例外にせず空の一覧を返す")]
    public async Task ExecuteAsync_NoMatch_ReturnsEmptyList()
    {
        _orderRepository
            .Setup(r => r.SearchAsync(It.IsAny<DateOnly?>(), It.IsAny<string?>()))
            .ReturnsAsync([]);

        var orders = await _interactor.ExecuteAsync(new OrderHistorySearchParam(new DateOnly(2020, 1, 1), null));

        Assert.HasCount(0, orders);
    }
}