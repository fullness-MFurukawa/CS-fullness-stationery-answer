using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class LoginResponseAdapterTests
{
    private LoginResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new LoginResponseAdapter();
    }

    [TestMethod(DisplayName = "ログイン実行結果をレスポンスへ変換する")]
    public void ToSource_ConvertsResultToResponse()
    {
        var department = new Department(Guid.NewGuid(), "販売管理部");
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", department);
        var account = new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed-password", employee);
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        var result = new EmployeeLoginResult(account, token);

        var response = _adapter.ToSource(result);

        Assert.AreEqual("fullness", response.AccountName);
        Assert.AreEqual("フルネス太郎", response.EmployeeName);
        Assert.AreEqual("dummy.jwt.token", response.AccessToken);
    }

    [TestMethod(DisplayName = "ToStringにアクセストークンを含めない")]
    public void ToString_DoesNotExposeAccessToken()
    {
        var department = new Department(Guid.NewGuid(), "販売管理部");
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", department);
        var account = new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed-password", employee);
        var token = new AccessToken("secret.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        var result = new EmployeeLoginResult(account, token);

        var response = _adapter.ToSource(result);
        var text = response.ToString();

        Assert.DoesNotContain("secret.jwt.token", text);
        Assert.Contains("fullness", text);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new LoginResponse("fullness", "フルネス太郎", "dummy.jwt.token");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}