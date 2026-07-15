using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class DashboardControllerTests
{
    private Mock<IDashboardSummaryUsecase> _dashboardSummaryUsecase = null!;
    private DashboardController _controller = null!;

    private DashboardSummary _summary = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _summary = new DashboardSummary(
            ProductCount: 13,
            CategoryCount: 3,
            OrderCount: 4,
            TotalSales: 4360,
            StatusCounts:
            [
                new OrderStatusCount(1, "注文済", 1),
                new OrderStatusCount(2, "入金済", 0),
                new OrderStatusCount(3, "配送中", 1),
                new OrderStatusCount(4, "完了", 2),
            ]);

        _dashboardSummaryUsecase = new Mock<IDashboardSummaryUsecase>();
        _dashboardSummaryUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(_summary);

        _controller = new DashboardController(
            _dashboardSummaryUsecase.Object,
            new DashboardSummaryResponseAdapter());
    }

    [TestMethod(DisplayName = "ダッシュボード集計は200と集計結果を返す")]
    public async Task GetSummaryAsync_ReturnsOkWithSummary()
    {
        var result = await _controller.GetSummaryAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as DashboardSummaryResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual(13, response!.ProductCount);
        Assert.AreEqual(3, response.CategoryCount);
        Assert.AreEqual(4, response.OrderCount);
        Assert.AreEqual(4360, response.TotalSales);
    }

    [TestMethod(DisplayName = "ダッシュボード集計はステータス別の件数を返す")]
    public async Task GetSummaryAsync_ReturnsStatusCounts()
    {
        var result = await _controller.GetSummaryAsync();

        var response = (result.Result as OkObjectResult)!.Value as DashboardSummaryResponse;

        Assert.IsNotNull(response);
        Assert.HasCount(4, response!.StatusCounts);
        Assert.AreEqual("注文済", response.StatusCounts[0].Name);
        Assert.AreEqual(1, response.StatusCounts[0].Count);
        Assert.AreEqual(0, response.StatusCounts[1].Count);
    }

    [TestMethod(DisplayName = "ダッシュボード集計はユースケースを1回だけ呼び出す")]
    public async Task GetSummaryAsync_CallsUsecaseOnce()
    {
        await _controller.GetSummaryAsync();

        _dashboardSummaryUsecase.Verify(u => u.ExecuteAsync(), Times.Once);
    }
}