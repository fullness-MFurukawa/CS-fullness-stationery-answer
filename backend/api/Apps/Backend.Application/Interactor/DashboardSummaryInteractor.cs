using Backend.Application.Results;
using Backend.Application.Usecases;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// 補助:ダッシュボード集計のユースケース実装
/// </summary>
/// <remarks>
/// 商品・カテゴリ・注文の各リポジトリから集計値を取得し、1つの結果へまとめる。
/// 件数や合計はデータベース側で集計するため、全件を取得しない。
/// </remarks>
public class DashboardSummaryInteractor : IDashboardSummaryUsecase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    /// <param name="orderRepository">注文のリポジトリ</param>
    /// <param name="orderStatusRepository">注文ステータスのリポジトリ</param>
    public DashboardSummaryInteractor(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        IOrderRepository orderRepository,
        IOrderStatusRepository orderStatusRepository)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
    }

    /// <summary>
    /// ダッシュボードに表示する集計値を取得する
    /// </summary>
    /// <returns>集計結果</returns>
    public async Task<DashboardSummary> ExecuteAsync()
    {
        var productCount = await _productRepository.CountAsync();
        var categoryCount = await _productCategoryRepository.CountAsync();
        var orderCount = await _orderRepository.CountAsync();
        var totalSales = await _orderRepository.SumAmountTotalAsync();
        var countByStatus = await _orderRepository.CountByStatusAsync();
        var statuses = await _orderStatusRepository.FindAllAsync();

        // すべてのステータスを表示対象とし、該当する注文が無い場合は0件として扱う
        var statusCounts = statuses
            .Select(status => new OrderStatusCount(
                status.Id,
                status.Name,
                countByStatus.TryGetValue(status.Id, out var count) ? count : 0))
            .ToList();

        return new DashboardSummary(
            productCount,
            categoryCount,
            orderCount,
            totalSales,
            statusCounts);
    }
}