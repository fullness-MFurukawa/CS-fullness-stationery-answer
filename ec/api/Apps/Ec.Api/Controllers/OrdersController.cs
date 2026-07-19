using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 注文のAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/ec/orders")]
[Produces("application/json")]
[Tags("注文")]
public class OrdersController : EcControllerBase
{
    private readonly IOrderCreateUsecase _orderCreateUsecase;
    private readonly IOrderHistorySearchUsecase _orderHistorySearchUsecase;
    private readonly IOrderDetailUsecase _orderDetailUsecase;
    private readonly OrderCreateRequestAdapter _requestAdapter;
    private readonly OrderResponseAdapter _responseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderCreateUsecase">購入確定のユースケース</param>
    /// <param name="orderHistorySearchUsecase">購入履歴一覧取得のユースケース</param>
    /// <param name="orderDetailUsecase">購入履歴詳細取得のユースケース</param>
    /// <param name="requestAdapter">購入確定のリクエストアダプタ</param>
    /// <param name="responseAdapter">注文のレスポンスアダプタ</param>
    public OrdersController(
        IOrderCreateUsecase orderCreateUsecase,
        IOrderHistorySearchUsecase orderHistorySearchUsecase,
        IOrderDetailUsecase orderDetailUsecase,
        OrderCreateRequestAdapter requestAdapter,
        OrderResponseAdapter responseAdapter)
    {
        _orderCreateUsecase = orderCreateUsecase;
        _orderHistorySearchUsecase = orderHistorySearchUsecase;
        _orderDetailUsecase = orderDetailUsecase;
        _requestAdapter = requestAdapter;
        _responseAdapter = responseAdapter;
    }

    /// <summary>
    /// UC005:注文を確定する
    /// </summary>
    /// <param name="request">購入確定のリクエスト</param>
    /// <returns>確定した注文</returns>
    /// <remarks>
    /// 在庫を引き当てて注文を登録する。在庫不足の場合は400を返す。
    /// 顧客は認証済みのトークンから特定する。
    /// </remarks>
    [EndpointSummary("購入確定")]
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> CreateAsync(OrderCreateRequest request)
    {
        var param = _requestAdapter.ToParam(request, GetCurrentCustomerId());
        var order = await _orderCreateUsecase.ExecuteAsync(param);
        var response = _responseAdapter.ToSource(order);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// UC007:購入履歴の一覧を取得する
    /// </summary>
    /// <returns>認証済みの顧客の注文一覧（新しい順）</returns>
    [EndpointSummary("購入履歴一覧の取得")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> SearchAsync()
    {
        var orders = await _orderHistorySearchUsecase.ExecuteAsync(GetCurrentCustomerId());
        var response = orders.Select(_responseAdapter.ToSource).ToList();
        return Ok(response);
    }

    /// <summary>
    /// UC007:購入履歴の詳細を取得する
    /// </summary>
    /// <param name="orderId">注文識別ID(uuid)</param>
    /// <returns>該当する注文</returns>
    /// <remarks>他人の注文は取得できず404を返す。</remarks>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("購入履歴詳細の取得")]
    public async Task<ActionResult<OrderResponse>> GetAsync(Guid orderId)
    {
        var order = await _orderDetailUsecase.ExecuteAsync(
            new OrderDetailParam(orderId, GetCurrentCustomerId()));
        var response = _responseAdapter.ToSource(order);
        return Ok(response);
    }
}