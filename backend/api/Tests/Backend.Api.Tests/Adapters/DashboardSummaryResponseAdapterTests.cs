using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class DashboardSummaryResponseAdapterTests
{
    private DashboardSummaryResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new DashboardSummaryResponseAdapter();
    }

    [TestMethod(DisplayName = "集計結果をレスポンスへ変換する")]
    public void ToSource_ConvertsSummaryToResponse()
    {
        var summary = new DashboardSummary(
            ProductCount: 13,
            CategoryCount: 3,
            OrderCount: 4,
            TotalSales: 4360,
            StatusCounts:
            [
                new OrderStatusCount(1, "注文済", 1),
                new OrderStatusCount(4, "完了", 2),
            ]);

        var response = _adapter.ToSource(summary);

        Assert.AreEqual(13, response.ProductCount);
        Assert.AreEqual(3, response.CategoryCount);
        Assert.AreEqual(4, response.OrderCount);
        Assert.AreEqual(4360, response.TotalSales);
    }

    [TestMethod(DisplayName = "ステータス別の件数をレスポンスへ変換する")]
    public void ToSource_ConvertsStatusCounts()
    {
        var summary = new DashboardSummary(
            ProductCount: 0,
            CategoryCount: 0,
            OrderCount: 0,
            TotalSales: 0,
            StatusCounts:
            [
                new OrderStatusCount(1, "注文済", 1),
                new OrderStatusCount(4, "完了", 2),
            ]);

        var response = _adapter.ToSource(summary);

        Assert.HasCount(2, response.StatusCounts);
        Assert.AreEqual(1, response.StatusCounts[0].OrderStatusId);
        Assert.AreEqual("注文済", response.StatusCounts[0].Name);
        Assert.AreEqual(1, response.StatusCounts[0].Count);
        Assert.AreEqual(4, response.StatusCounts[1].OrderStatusId);
        Assert.AreEqual(2, response.StatusCounts[1].Count);
    }

    [TestMethod(DisplayName = "ステータス別の件数が空でも変換できる")]
    public void ToSource_EmptyStatusCounts_ReturnsEmptyList()
    {
        var summary = new DashboardSummary(0, 0, 0, 0, []);

        var response = _adapter.ToSource(summary);

        Assert.HasCount(0, response.StatusCounts);
    }

    [TestMethod(DisplayName = "レスポンスから集計結果への変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new DashboardSummaryResponse(13, 3, 4, 4360, []);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}