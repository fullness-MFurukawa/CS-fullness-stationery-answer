using Backend.Api.Adapters;
using Backend.Api.Authentication;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class AuthControllerTests
{
    private Mock<IEmployeeLoginUsecase> _employeeLoginUsecase = null!;
    private AuthController _controller = null!;

    private EmployeeAccount _account = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    /// <remarks>
    /// Response.Cookies を操作するため、ControllerContext に HttpContext を差し込む。
    /// </remarks>
    [TestInitialize]
    public void SetUp()
    {
        var department = new Department(Guid.NewGuid(), "販売管理部");
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", department);
        _account = new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed", employee);

        _employeeLoginUsecase = new Mock<IEmployeeLoginUsecase>();

        var cookieOptions = Options.Create(new AuthCookieOptions { Name = "auth", Secure = false });

        _controller = new AuthController(
            _employeeLoginUsecase.Object,
            new LoginRequestAdapter(),
            new LoginResponseAdapter(),
            new AuthCookie(cookieOptions))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    [TestMethod(DisplayName = "ログイン成功は200と担当者情報を返す")]
    public async Task LoginAsync_Success_ReturnsOkWithAccount()
    {
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        _employeeLoginUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Backend.Application.Params.EmployeeLoginParam>()))
            .ReturnsAsync(new EmployeeLoginResult(_account, token));

        var request = new LoginRequest("fullness", "Password123");
        var result = await _controller.LoginAsync(request);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as LoginResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("fullness", response!.AccountName);
    }

    [TestMethod(DisplayName = "ログイン成功時は認証Cookieを設定する")]
    public async Task LoginAsync_Success_SetsAuthCookie()
    {
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        _employeeLoginUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Backend.Application.Params.EmployeeLoginParam>()))
            .ReturnsAsync(new EmployeeLoginResult(_account, token));

        await _controller.LoginAsync(new LoginRequest("fullness", "Password123"));

        // Set-Cookie ヘッダに auth クッキーが設定されていることを確認する
        var setCookie = _controller.Response.Headers.SetCookie.ToString();
        Assert.Contains("auth=", setCookie);
        Assert.Contains("httponly", setCookie.ToLowerInvariant());
    }

    [TestMethod(DisplayName = "ログインレスポンスにトークンに相当するプロパティが存在しない")]
    public void LoginResponse_HasNoTokenProperty()
    {
        var hasTokenProperty = typeof(LoginResponse)
            .GetProperties()
            .Any(p => p.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));

        Assert.IsFalse(hasTokenProperty);
    }

    [TestMethod(DisplayName = "ログアウトは204を返し認証Cookieを失効させる")]
    public void Logout_ReturnsNoContentAndExpiresCookie()
    {
        var result = _controller.Logout();

        Assert.IsInstanceOfType<NoContentResult>(result);

        var setCookie = _controller.Response.Headers.SetCookie.ToString();
        Assert.Contains("auth=", setCookie);
    }
}