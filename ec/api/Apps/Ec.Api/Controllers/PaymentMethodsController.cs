using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 支払い方法のAPI
/// </summary>
[ApiController]
[Route("api/ec/payment-methods")]
[Produces("application/json")]
[Tags("支払い方法")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodSearchUsecase _paymentMethodSearchUsecase;
    private readonly PaymentMethodResponseAdapter _responseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="paymentMethodSearchUsecase">支払い方法一覧取得のユースケース</param>
    /// <param name="responseAdapter">支払い方法のレスポンスアダプタ</param>
    public PaymentMethodsController(
        IPaymentMethodSearchUsecase paymentMethodSearchUsecase,
        PaymentMethodResponseAdapter responseAdapter)
    {
        _paymentMethodSearchUsecase = paymentMethodSearchUsecase;
        _responseAdapter = responseAdapter;
    }

    /// <summary>
    /// UC005:支払い方法の一覧を取得する
    /// </summary>
    /// <returns>支払い方法の一覧</returns>
    /// <remarks>購入確認画面(FP009)のプルダウンに表示する。</remarks>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentMethodResponse>>> SearchAsync()
    {
        var methods = await _paymentMethodSearchUsecase.ExecuteAsync();
        var response = methods.Select(_responseAdapter.ToSource).ToList();
        return Ok(response);
    }
}