using Backend.Application.Interactor;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class DashboardSummaryInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IOrderStatusRepository> _orderStatusRepository = null!;
    private DashboardSummaryInteractor _interactor = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _productRepository = new Mock<IProductRepository>();
        _productRepository.Setup(r => r.CountAsync()).ReturnsAsync(13);

        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _productCategoryRepository.Setup(r => r.CountAsync()).ReturnsAsync(3);

        _orderRepository = new Mock<IOrderRepository>();
        _orderRepository.Setup(r => r.CountAsync()).ReturnsAsync(4);
        _orderRepository.Setup(r => r.SumAmountTotalAsync()).ReturnsAsync(4360);
        _orderRepository
            .Setup(r => r.CountByStatusAsync())
            .ReturnsAsync(new Dictionary<int, int> { [1] = 1, [3] = 1, [4] = 2 });

        _orderStatusRepository = new Mock<IOrderStatusRepository>();
        _orderStatusRepository
            .Setup(r => r.FindAllAsync())
            .ReturnsAsync(new List<OrderStatus>
            {
                new(1, "注文済"),
                new(2, "入金済"),
                new(3, "配送中"),
                new(4, "完了"),
            });

        _interactor = new DashboardSummaryInteractor(
            _productRepository.Object,
            _productCategoryRepository.Object,
            _orderRepository.Object,
            _orderStatusRepository.Object);
    }

    [TestMethod(DisplayName = "各リポジトリの集計値を1つの結果へまとめる")]
    public async Task ExecuteAsync_ReturnsAggregatedSummary()
    {
        var summary = await _interactor.ExecuteAsync();

        Assert.AreEqual(13, summary.ProductCount);
        Assert.AreEqual(3, summary.CategoryCount);
        Assert.AreEqual(4, summary.OrderCount);
        Assert.AreEqual(4360, summary.TotalSales);
    }

    [TestMethod(DisplayName = "ステータス別の件数にステータス名を付与する")]
    public async Task ExecuteAsync_AssignsStatusNames()
    {
        var summary = await _interactor.ExecuteAsync();

        var ordered = summary.StatusCounts.OrderBy(s => s.OrderStatusId).ToList();
        Assert.AreEqual("注文済", ordered[0].Name);
        Assert.AreEqual("入金済", ordered[1].Name);
        Assert.AreEqual("配送中", ordered[2].Name);
        Assert.AreEqual("完了", ordered[3].Name);
    }

    [TestMethod(DisplayName = "該当する注文が無いステータスは0件として扱う")]
    public async Task ExecuteAsync_StatusWithoutOrders_CountsZero()
    {
        var summary = await _interactor.ExecuteAsync();

        // 「入金済」(ID:2)は集計結果に含まれないため0件となる
        var paid = summary.StatusCounts.Single(s => s.OrderStatusId == 2);
        Assert.AreEqual(0, paid.Count);
    }

    [TestMethod(DisplayName = "すべてのステータスを結果に含める")]
    public async Task ExecuteAsync_IncludesAllStatuses()
    {
        var summary = await _interactor.ExecuteAsync();

        Assert.HasCount(4, summary.StatusCounts);
    }

    [TestMethod(DisplayName = "ステータス別の件数を集計結果から反映する")]
    public async Task ExecuteAsync_ReflectsStatusCounts()
    {
        var summary = await _interactor.ExecuteAsync();

        Assert.AreEqual(1, summary.StatusCounts.Single(s => s.OrderStatusId == 1).Count);
        Assert.AreEqual(1, summary.StatusCounts.Single(s => s.OrderStatusId == 3).Count);
        Assert.AreEqual(2, summary.StatusCounts.Single(s => s.OrderStatusId == 4).Count);
    }

    [TestMethod(DisplayName = "注文が存在しない場合は各集計値が0になる")]
    public async Task ExecuteAsync_NoOrders_ReturnsZeroCounts()
    {
        _orderRepository.Setup(r => r.CountAsync()).ReturnsAsync(0);
        _orderRepository.Setup(r => r.SumAmountTotalAsync()).ReturnsAsync(0);
        _orderRepository
            .Setup(r => r.CountByStatusAsync())
            .ReturnsAsync(new Dictionary<int, int>());

        var summary = await _interactor.ExecuteAsync();

        Assert.AreEqual(0, summary.OrderCount);
        Assert.AreEqual(0, summary.TotalSales);
        Assert.IsTrue(summary.StatusCounts.All(s => s.Count == 0));
        // 注文が無くても、すべてのステータスは表示対象とする
        Assert.HasCount(4, summary.StatusCounts);
    }
}