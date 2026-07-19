using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 顧客の認証のAPI
/// </summary>
[ApiController]
[Route("api/ec/auth")]
[Produces("application/json")]
[Tags("顧客認証")]
public class AuthController : ControllerBase
{
    private readonly ICustomerLoginUsecase _customerLoginUsecase;
    private readonly LoginRequestAdapter _loginRequestAdapter;
    private readonly LoginResponseAdapter _loginResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerLoginUsecase">顧客ログインのユースケース</param>
    /// <param name="loginRequestAdapter">ログインのリクエストアダプタ</param>
    /// <param name="loginResponseAdapter">ログインのレスポンスアダプタ</param>
    public AuthController(
        ICustomerLoginUsecase customerLoginUsecase,
        LoginRequestAdapter loginRequestAdapter,
        LoginResponseAdapter loginResponseAdapter)
    {
        _customerLoginUsecase = customerLoginUsecase;
        _loginRequestAdapter = loginRequestAdapter;
        _loginResponseAdapter = loginResponseAdapter;
    }

    /// <summary>
    /// UC002:顧客を認証する
    /// </summary>
    /// <param name="request">ログインのリクエスト</param>
    /// <returns>ログインした顧客の情報とアクセストークン</returns>
    /// <remarks>
    /// アクセストークンはレスポンスボディで返す。
    /// メールアドレスの誤りとパスワードの誤りは区別せず、いずれも401とする。
    /// </remarks>
    [EndpointSummary("顧客ログイン")]
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var param = _loginRequestAdapter.ToDomain(request);
        var result = await _customerLoginUsecase.ExecuteAsync(param);
        var response = _loginResponseAdapter.ToSource(result);
        return Ok(response);
    }
}