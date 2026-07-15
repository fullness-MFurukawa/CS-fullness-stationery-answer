namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// ダッシュボード集計のレスポンス
/// </summary>
/// <param name="ProductCount">有効な商品の件数(論理削除を除く)</param>
/// <param name="CategoryCount">商品カテゴリの件数</param>
/// <param name="OrderCount">注文の件数</param>
/// <param name="TotalSales">売上合計</param>
/// <param name="StatusCounts">注文ステータスごとの注文件数</param>
public sealed record DashboardSummaryResponse(
    int ProductCount,
    int CategoryCount,
    int OrderCount,
    int TotalSales,
    IReadOnlyList<OrderStatusCountResponse> StatusCounts);