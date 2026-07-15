namespace Backend.Application.Results;

/// <summary>
/// 補助:ダッシュボードの集計結果
/// </summary>
/// <param name="ProductCount">有効な商品の件数(論理削除を除く)</param>
/// <param name="CategoryCount">商品カテゴリの件数</param>
/// <param name="OrderCount">注文の件数</param>
/// <param name="TotalSales">売上合計</param>
/// <param name="StatusCounts">注文ステータスごとの注文件数</param>
public sealed record DashboardSummary(
    int ProductCount,
    int CategoryCount,
    int OrderCount,
    int TotalSales,
    IReadOnlyList<OrderStatusCount> StatusCounts);