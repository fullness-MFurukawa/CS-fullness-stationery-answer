using Backend.Application.Results;

namespace Backend.Application.Usecases;

/// <summary>
/// 補助:ダッシュボード集計のユースケース
/// </summary>
/// <remarks>
/// メニュー画面で、商品・カテゴリ・注文の状況を一覧表示するために使用する。
/// </remarks>
public interface IDashboardSummaryUsecase
{
    /// <summary>
    /// ダッシュボードに表示する集計値を取得する
    /// </summary>
    /// <returns>集計結果</returns>
    Task<DashboardSummary> ExecuteAsync();
}