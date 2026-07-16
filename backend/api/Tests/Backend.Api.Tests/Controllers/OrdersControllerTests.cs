using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class OrdersControllerTests
{
    private Mock<IOrderHistorySearchUsecase> _orderHistorySearchUsecase = null!;
    private Mock<IOrderStatusUpdateUsecase> _orderStatusUpdateUsecase = null!;
    private OrdersController _controller = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    /// <remarks>
    /// アダプタは単純な変換処理のため、モック化せず実インスタンスを使用する。
    /// </remarks>
    [TestInitialize]
    public void SetUp()
    {
        _orderHistorySearchUsecase = new Mock<IOrderHistorySearchUsecase>();
        _orderStatusUpdateUsecase = new Mock<IOrderStatusUpdateUsecase>();

        _controller = new OrdersController(
            _orderHistorySearchUsecase.Object,
            _orderStatusUpdateUsecase.Object,
            new OrderStatusUpdateRequestAdapter(),
            new OrderResponseAdapter(new OrderDetailResponseAdapter()));
    }

    /// <summary>
    /// テスト用の注文を1件生成する
    /// </summary>
    private static Order CreateOrder(string statusName = "注文済", int amountTotal = 120)
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var product = new Product(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, null, category,
            new ProductStock(Guid.NewGuid(), 10));

        var customer = new Customer(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", null,
            "090-1234-5678", "test@example.com", "testuser", "hashed", DateTime.Now);

        var status = new OrderStatus(1, statusName);
        var payment = new PaymentMethod(1, "現金");
        var details = new List<OrderDetail> { new(1, product, 1) };

        return new Order(Guid.NewGuid(), DateTime.Now, amountTotal, customer, status, payment, details);
    }

    [TestMethod(DisplayName = "購入履歴検索は200と注文一覧を返す")]
    public async Task SearchAsync_ReturnsOkWithOrders()
    {
        var orders = new List<Order> { CreateOrder("完了"), CreateOrder("配送中") };
        _orderHistorySearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderHistorySearchParam>()))
            .ReturnsAsync(orders);

        var result = await _controller.SearchAsync(null, null, null);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as IReadOnlyList<OrderResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(2, response!);
    }

    [TestMethod(DisplayName = "購入履歴検索は該当0件で空配列を返す")]
    public async Task SearchAsync_NoOrders_ReturnsEmptyList()
    {
        _orderHistorySearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderHistorySearchParam>()))
            .ReturnsAsync(new List<Order>());

        var result = await _controller.SearchAsync(null, null, null);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var response = ok!.Value as IReadOnlyList<OrderResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(0, response!);
    }

    [TestMethod(DisplayName = "購入履歴検索は検索条件をユースケースへ渡す")]
    public async Task SearchAsync_PassesConditionsToUsecase()
    {
        var orderDate = new DateOnly(2024, 5, 12);
        const string accountName = "testuser";
        _orderHistorySearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderHistorySearchParam>()))
            .ReturnsAsync(new List<Order>());

        await _controller.SearchAsync(orderDate, accountName, null);

        _orderHistorySearchUsecase.Verify(
            u => u.ExecuteAsync(It.Is<OrderHistorySearchParam>(
                p => p.OrderDate == orderDate && p.CustomerAccountName == accountName)),
            Times.Once);
    }

    [TestMethod(DisplayName = "購入履歴検索は注文ステータスIDをユースケースへ渡す")]
    public async Task SearchAsync_PassesOrderStatusIdToUsecase()
    {
        _orderHistorySearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderHistorySearchParam>()))
            .ReturnsAsync(new List<Order>());

        await _controller.SearchAsync(null, null, 3);

        _orderHistorySearchUsecase.Verify(
            u => u.ExecuteAsync(It.Is<OrderHistorySearchParam>(p => p.OrderStatusId == 3)),
            Times.Once);
    }

    [TestMethod(DisplayName = "購入履歴検索はすべての検索条件をユースケースへ渡す")]
    public async Task SearchAsync_PassesAllConditionsToUsecase()
    {
        var orderDate = new DateOnly(2024, 5, 12);
        const string accountName = "testuser";
        _orderHistorySearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderHistorySearchParam>()))
            .ReturnsAsync(new List<Order>());

        await _controller.SearchAsync(orderDate, accountName, 3);

        _orderHistorySearchUsecase.Verify(
            u => u.ExecuteAsync(It.Is<OrderHistorySearchParam>(
                p => p.OrderDate == orderDate
                  && p.CustomerAccountName == accountName
                  && p.OrderStatusId == 3)),
            Times.Once);
    }

    [TestMethod(DisplayName = "注文ステータス更新は200と更新後の注文を返す")]
    public async Task UpdateStatusAsync_ReturnsOkWithOrder()
    {
        var order = CreateOrder("入金済");
        _orderStatusUpdateUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderStatusUpdateParam>()))
            .ReturnsAsync(order);

        var request = new OrderStatusUpdateRequest(2);
        var result = await _controller.UpdateStatusAsync(order.Id, request);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as OrderResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("入金済", response!.StatusName);
    }

    [TestMethod(DisplayName = "注文ステータス更新はルートの注文IDを優先してユースケースへ渡す")]
    public async Task UpdateStatusAsync_UsesRouteOrderId()
    {
        var routeOrderId = Guid.NewGuid();
        var order = CreateOrder();
        _orderStatusUpdateUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderStatusUpdateParam>()))
            .ReturnsAsync(order);

        // ボディには別のOrderIdを入れ、ルートの値が優先されることを確認する
        var request = new OrderStatusUpdateRequest(2) { OrderId = Guid.NewGuid() };
        await _controller.UpdateStatusAsync(routeOrderId, request);

        _orderStatusUpdateUsecase.Verify(
            u => u.ExecuteAsync(It.Is<OrderStatusUpdateParam>(p => p.OrderId == routeOrderId)),
            Times.Once);
    }
}