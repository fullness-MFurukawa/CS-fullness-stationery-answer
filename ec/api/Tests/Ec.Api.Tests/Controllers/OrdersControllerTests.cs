using System.Security.Claims;
using Ec.Api.Adapters;
using Ec.Api.Controllers;
using Ec.Api.ViewModels.Requests;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Exceptions;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace Ec.Api.Tests.Controllers;

[TestClass]
[TestCategory("Ec.Api.Controllers")]
public class OrdersControllerTests
{
    private Mock<IOrderCreateUsecase> _createUsecase = null!;
    private Mock<IOrderHistorySearchUsecase> _historyUsecase = null!;
    private Mock<IOrderDetailUsecase> _detailUsecase = null!;
    private OrdersController _controller = null!;
    private Guid _customerId;

    /// <summary>
    /// テスト用の注文を生成する
    /// </summary>
    private Order CreateOrder()
    {
        var customer = new Customer(
            _customerId, "山田太郎", "ヤマダタロウ", "東京都新宿区", null,
            "090-1234-5678", "taro@example.com", "taro01", "hashed", DateTime.Now);
        var product = new Product(
            Guid.NewGuid(), "ボールペン", 120, null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));
        return Order.Create(
            Guid.NewGuid(),
            customer,
            new OrderStatus(OrderStatus.OrderedId, "注文済"),
            new PaymentMethod(1, "現金"),
            [new OrderDetail(product, 1)],
            DateTime.Now);
    }

    [TestInitialize]
    public void SetUp()
    {
        _customerId = Guid.NewGuid();
        _createUsecase = new Mock<IOrderCreateUsecase>();
        _historyUsecase = new Mock<IOrderHistorySearchUsecase>();
        _detailUsecase = new Mock<IOrderDetailUsecase>();

        var responseAdapter = new OrderResponseAdapter(new OrderDetailResponseAdapter());
        _controller = new OrdersController(
            _createUsecase.Object,
            _historyUsecase.Object,
            _detailUsecase.Object,
            new OrderCreateRequestAdapter(),
            responseAdapter);

        // 認証済みの状態をHttpContextへ差し込む。
        // EcControllerBase.GetCurrentCustomerId が sub クレームを読むため
        var identity = new ClaimsIdentity([new Claim("sub", _customerId.ToString())], "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
        };
    }

    [TestMethod(DisplayName = "購入確定は201と注文を返す")]
    public async Task CreateAsync_Success_ReturnsCreated()
    {
        _createUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderCreateParam>()))
            .ReturnsAsync(CreateOrder());

        var request = new OrderCreateRequest(1, [new OrderItemRequest(Guid.NewGuid(), 1)]);
        var result = await _controller.CreateAsync(request);

        var created = result.Result as ObjectResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);
    }

    [TestMethod(DisplayName = "購入確定では認証済みの顧客IDがユースケースへ渡される")]
    public async Task CreateAsync_PassesAuthenticatedCustomerId()
    {
        OrderCreateParam? captured = null;
        _createUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderCreateParam>()))
            .Callback<OrderCreateParam>(p => captured = p)
            .ReturnsAsync(CreateOrder());

        var request = new OrderCreateRequest(1, [new OrderItemRequest(Guid.NewGuid(), 1)]);
        await _controller.CreateAsync(request);

        // フロントから顧客IDを送らず、トークンのsubから取ることの検証
        Assert.IsNotNull(captured);
        Assert.AreEqual(_customerId, captured!.CustomerId);
    }

    [TestMethod(DisplayName = "購入履歴一覧は認証済みの顧客IDで取得する")]
    public async Task SearchAsync_UsesAuthenticatedCustomerId()
    {
        Guid captured = Guid.Empty;
        _historyUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid>()))
            .Callback<Guid>(id => captured = id)
            .ReturnsAsync([CreateOrder()]);

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(_customerId, captured);
    }

    [TestMethod(DisplayName = "購入履歴詳細は注文IDと顧客IDでユースケースを呼ぶ")]
    public async Task GetAsync_PassesOrderIdAndCustomerId()
    {
        OrderDetailParam? captured = null;
        var orderId = Guid.NewGuid();
        _detailUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<OrderDetailParam>()))
            .Callback<OrderDetailParam>(p => captured = p)
            .ReturnsAsync(CreateOrder());

        await _controller.GetAsync(orderId);

        Assert.IsNotNull(captured);
        Assert.AreEqual(orderId, captured!.OrderId);
        Assert.AreEqual(_customerId, captured.CustomerId);
    }

    [TestMethod(DisplayName = "subクレームがない場合はAuthenticationFailedExceptionをスローする")]
    public async Task CreateAsync_NoSubClaim_ThrowsAuthenticationFailedException()
    {
        // 認証情報のない状態に差し替える
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext(),
        };

        var request = new OrderCreateRequest(1, [new OrderItemRequest(Guid.NewGuid(), 1)]);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _controller.CreateAsync(request));
    }
}