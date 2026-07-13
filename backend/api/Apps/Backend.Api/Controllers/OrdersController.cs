using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Params;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 注文のAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/orders")]
[Produces("application/json")]
[Tags("注文")]
public class OrdersController : ControllerBase
{
    private readonly IOrderHistorySearchUsecase _orderHistorySearchUsecase;
    private readonly IOrderStatusUpdateUsecase _orderStatusUpdateUsecase;
    private readonly OrderStatusUpdateRequestAdapter _orderStatusUpdateRequestAdapter;
    private readonly OrderResponseAdapter _orderResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderHistorySearchUsecase">購入履歴検索のユースケース</param>
    /// <param name="orderStatusUpdateUsecase">注文ステータス更新のユースケース</param>
    /// <param name="orderStatusUpdateRequestAdapter">注文ステータス更新のリクエストアダプタ</param>
    /// <param name="orderResponseAdapter">注文のレスポンスアダプタ</param>
    public OrdersController(
        IOrderHistorySearchUsecase orderHistorySearchUsecase,
        IOrderStatusUpdateUsecase orderStatusUpdateUsecase,
        OrderStatusUpdateRequestAdapter orderStatusUpdateRequestAdapter,
        OrderResponseAdapter orderResponseAdapter)
    {
        _orderHistorySearchUsecase = orderHistorySearchUsecase;
        _orderStatusUpdateUsecase = orderStatusUpdateUsecase;
        _orderStatusUpdateRequestAdapter = orderStatusUpdateRequestAdapter;
        _orderResponseAdapter = orderResponseAdapter;
    }

    /// <summary>
    /// UC015:購入履歴を検索する
    /// </summary>
    /// <param name="orderDate">購入日。指定しない場合は条件に含めない</param>
    /// <param name="customerAccountName">顧客アカウント名。指定しない場合は条件に含めない</param>
    /// <returns>条件に一致する注文の一覧（新しい順）</returns>
    /// <remarks>両方を省略した場合はすべての注文を返す。</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> SearchAsync(
        [FromQuery] DateOnly? orderDate,
        [FromQuery] string? customerAccountName)
    {
        var param = new OrderHistorySearchParam(orderDate, customerAccountName);
        var orders = await _orderHistorySearchUsecase.ExecuteAsync(param);

        // 該当0件は正常系のため、空配列を返す
        var response = orders.Select(_orderResponseAdapter.ToSource).ToList();

        return Ok(response);
    }

    /// <summary>
    /// UC016:注文ステータスを更新する
    /// </summary>
    /// <param name="orderId">更新対象の注文識別ID(uuid)</param>
    /// <param name="request">注文ステータス更新のリクエスト</param>
    /// <returns>ステータスを更新した注文</returns>
    [HttpPut("{orderId:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> UpdateStatusAsync(
        Guid orderId,
        OrderStatusUpdateRequest request)
    {
        // 対象の識別子はルートパラメータを正とし、リクエストボディの値は使用しない
        var param = _orderStatusUpdateRequestAdapter.ToDomain(request with { OrderId = orderId });
        var order = await _orderStatusUpdateUsecase.ExecuteAsync(param);

        var response = _orderResponseAdapter.ToSource(order);

        return Ok(response);
    }
}