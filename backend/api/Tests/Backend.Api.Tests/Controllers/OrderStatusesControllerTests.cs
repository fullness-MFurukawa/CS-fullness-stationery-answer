using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class OrderStatusesControllerTests
{
    private Mock<IOrderStatusSearchUsecase> _orderStatusSearchUsecase = null!;
    private OrderStatusesController _controller = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _orderStatusSearchUsecase = new Mock<IOrderStatusSearchUsecase>();

        _controller = new OrderStatusesController(
            _orderStatusSearchUsecase.Object,
            new OrderStatusResponseAdapter());
    }

    [TestMethod(DisplayName = "注文ステータス一覧は200と全ステータスを返す")]
    public async Task SearchAsync_ReturnsOkWithStatuses()
    {
        var statuses = new List<OrderStatus>
        {
            new(1, "注文済"),
            new(2, "入金済"),
            new(3, "配送中"),
            new(4, "完了"),
        };
        _orderStatusSearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(statuses);

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as IReadOnlyList<OrderStatusResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(4, response!);
        Assert.AreEqual(1, response![0].OrderStatusId);
        Assert.AreEqual("注文済", response[0].Name);
    }

    [TestMethod(DisplayName = "注文ステータス一覧は該当0件で空配列を返す")]
    public async Task SearchAsync_NoStatuses_ReturnsEmptyList()
    {
        _orderStatusSearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(new List<OrderStatus>());

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var response = ok!.Value as IReadOnlyList<OrderStatusResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(0, response!);
    }
}