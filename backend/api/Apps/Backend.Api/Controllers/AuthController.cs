using Backend.Api.Adapters;
using Backend.Api.Authentication;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 担当者の認証のAPI
/// </summary>
[ApiController]
[Route("api/admin/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IEmployeeLoginUsecase _employeeLoginUsecase;
    private readonly LoginRequestAdapter _loginRequestAdapter;
    private readonly LoginResponseAdapter _loginResponseAdapter;
    private readonly AuthCookie _authCookie;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeLoginUsecase">担当者ログインのユースケース</param>
    /// <param name="loginRequestAdapter">ログインのリクエストアダプタ</param>
    /// <param name="loginResponseAdapter">ログインのレスポンスアダプタ</param>
    /// <param name="authCookie">認証Cookie</param>
    public AuthController(
        IEmployeeLoginUsecase employeeLoginUsecase,
        LoginRequestAdapter loginRequestAdapter,
        LoginResponseAdapter loginResponseAdapter,
        AuthCookie authCookie)
    {
        _employeeLoginUsecase = employeeLoginUsecase;
        _loginRequestAdapter = loginRequestAdapter;
        _loginResponseAdapter = loginResponseAdapter;
        _authCookie = authCookie;
    }

    /// <summary>
    /// UC017:担当者を認証する
    /// </summary>
    /// <param name="request">ログインのリクエスト</param>
    /// <returns>ログインした担当者の情報</returns>
    /// <remarks>
    /// 認証に成功した場合、アクセストークンをHttpOnly Cookieへセットして返す。
    /// アカウント名の誤りとパスワードの誤りは区別せず、いずれも401とする。
    /// レスポンスボディにトークンを含めない。
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var param = _loginRequestAdapter.ToDomain(request);
        var result = await _employeeLoginUsecase.ExecuteAsync(param);

        Response.Cookies.Append(
            _authCookie.Name,
            result.Token.Value,
            _authCookie.Create(result.Token.ExpiresAt));

        var response = _loginResponseAdapter.ToSource(result.Account);

        return Ok(response);
    }

    /// <summary>
    /// UC018:ログアウトする
    /// </summary>
    /// <returns>レスポンスボディなし</returns>
    /// <remarks>
    /// 認証Cookieを失効させる。
    /// JWTはステートレスなため、サーバ側でトークンそのものを無効化しない。
    /// 有効期限（30分）が切れるまでトークン自体は有効である点は許容する。
    /// </remarks>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(_authCookie.Name, _authCookie.CreateForDelete());

        return NoContent();
    }
}