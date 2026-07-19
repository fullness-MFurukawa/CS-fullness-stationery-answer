using Ec.Api.Adapters;
using Ec.Api.Controllers;
using Ec.Api.ViewModels.Requests;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Params;
using Ec.Application.Results;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace Ec.Api.Tests.Controllers;

[TestClass]
[TestCategory("Ec.Api.Controllers")]
public class AuthControllerTests
{
    private Mock<ICustomerLoginUsecase> _customerLoginUsecase = null!;
    private AuthController _controller = null!;
    private Customer _customer = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _customer = new Customer(
            Guid.NewGuid(), "山田太郎", "ヤマダタロウ", "東京都新宿区", null,
            "090-1234-5678", "taro@example.com", "taro01", "hashed-password", DateTime.Now);

        _customerLoginUsecase = new Mock<ICustomerLoginUsecase>();

        _controller = new AuthController(
            _customerLoginUsecase.Object,
            new LoginRequestAdapter(),
            new LoginResponseAdapter())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
    }

    [TestMethod(DisplayName = "ログイン成功は200と顧客情報を返す")]
    public async Task LoginAsync_Success_ReturnsOkWithCustomer()
    {
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        _customerLoginUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<CustomerLoginParam>()))
            .ReturnsAsync(new CustomerLoginResult(_customer, token));

        var result = await _controller.LoginAsync(new LoginRequest("taro@example.com", "Password123"));

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        var response = ok.Value as LoginResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("山田太郎", response!.CustomerName);
    }

    [TestMethod(DisplayName = "ログイン成功時はレスポンスにアクセストークンを含める")]
    public async Task LoginAsync_Success_ReturnsAccessToken()
    {
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        _customerLoginUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<CustomerLoginParam>()))
            .ReturnsAsync(new CustomerLoginResult(_customer, token));

        var result = await _controller.LoginAsync(new LoginRequest("taro@example.com", "Password123"));

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var response = ok!.Value as LoginResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("dummy.jwt.token", response!.AccessToken);
    }

    [TestMethod(DisplayName = "リクエストの入力値がユースケースへ渡される")]
    public async Task LoginAsync_PassesParamToUsecase()
    {
        CustomerLoginParam? captured = null;
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        _customerLoginUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<CustomerLoginParam>()))
            .Callback<CustomerLoginParam>(p => captured = p)
            .ReturnsAsync(new CustomerLoginResult(_customer, token));

        await _controller.LoginAsync(new LoginRequest("taro@example.com", "Password123"));

        Assert.IsNotNull(captured);
        Assert.AreEqual("taro@example.com", captured!.MailAddress);
        Assert.AreEqual("Password123", captured.Password);
    }
}