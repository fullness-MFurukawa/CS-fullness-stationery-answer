using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 注文ステータスのAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/order-statuses")]
[Produces("application/json")]
public class OrderStatusesController : ControllerBase
{
    private readonly IOrderStatusSearchUsecase _orderStatusSearchUsecase;
    private readonly OrderStatusResponseAdapter _orderStatusResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderStatusSearchUsecase">注文ステータス一覧取得のユースケース</param>
    /// <param name="orderStatusResponseAdapter">注文ステータスのレスポンスアダプタ</param>
    public OrderStatusesController(
        IOrderStatusSearchUsecase orderStatusSearchUsecase,
        OrderStatusResponseAdapter orderStatusResponseAdapter)
    {
        _orderStatusSearchUsecase = orderStatusSearchUsecase;
        _orderStatusResponseAdapter = orderStatusResponseAdapter;
    }

    /// <summary>
    /// すべての注文ステータスを取得する
    /// </summary>
    /// <returns>注文ステータスの一覧</returns>
    /// <remarks>注文ステータス更新画面のプルダウンの選択肢として使用する。</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderStatusResponse>>> SearchAsync()
    {
        var statuses = await _orderStatusSearchUsecase.ExecuteAsync();

        var response = statuses.Select(_orderStatusResponseAdapter.ToSource).ToList();

        return Ok(response);
    }
}