using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// ダッシュボード集計結果とレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスから集計結果への復元は用途が存在しないため未サポートとする。
/// </remarks>
public class DashboardSummaryResponseAdapter : IEntityAdapter<DashboardSummaryResponse, DashboardSummary>
{
    /// <summary>
    /// レスポンスから集計結果へ変換する（未サポート）
    /// </summary>
    /// <param name="source">ダッシュボード集計のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public DashboardSummary ToDomain(DashboardSummaryResponse source)
        => throw new NotSupportedException("レスポンスから集計結果への復元は行えません。");

    /// <summary>
    /// 集計結果からレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ダッシュボードの集計結果</param>
    /// <returns>ダッシュボード集計のレスポンス</returns>
    public DashboardSummaryResponse ToSource(DashboardSummary domain)
        => new(
            domain.ProductCount,
            domain.CategoryCount,
            domain.OrderCount,
            domain.TotalSales,
            domain.StatusCounts
                .Select(s => new OrderStatusCountResponse(s.OrderStatusId, s.Name, s.Count))
                .ToList());
}