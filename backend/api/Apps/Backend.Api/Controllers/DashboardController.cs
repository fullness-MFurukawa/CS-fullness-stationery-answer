using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// ダッシュボードのAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/dashboard")]
[Produces("application/json")]
[Tags("ダッシュボード")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardSummaryUsecase _dashboardSummaryUsecase;
    private readonly DashboardSummaryResponseAdapter _dashboardSummaryResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="dashboardSummaryUsecase">ダッシュボード集計のユースケース</param>
    /// <param name="dashboardSummaryResponseAdapter">ダッシュボード集計のレスポンスアダプタ</param>
    public DashboardController(
        IDashboardSummaryUsecase dashboardSummaryUsecase,
        DashboardSummaryResponseAdapter dashboardSummaryResponseAdapter)
    {
        _dashboardSummaryUsecase = dashboardSummaryUsecase;
        _dashboardSummaryResponseAdapter = dashboardSummaryResponseAdapter;
    }

    /// <summary>
    /// ダッシュボードに表示する集計値を取得する（補助）
    /// </summary>
    /// <returns>商品数・カテゴリ数・注文件数・売上合計・ステータス別の注文件数</returns>
    /// <remarks>
    /// メニュー画面で管理業務の状況を把握するために使用する。
    /// 件数や合計はデータベース側で集計するため、全件を取得しない。
    /// </remarks>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummaryAsync()
    {
        var summary = await _dashboardSummaryUsecase.ExecuteAsync();

        var response = _dashboardSummaryResponseAdapter.ToSource(summary);

        return Ok(response);
    }
}