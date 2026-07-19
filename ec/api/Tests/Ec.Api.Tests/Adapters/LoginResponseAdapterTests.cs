using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Results;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class LoginResponseAdapterTests
{
    private LoginResponseAdapter _adapter = null!;

    /// <summary>
    /// テスト用の顧客を生成する
    /// </summary>
    private static Customer CreateCustomer()
        => new(
            Guid.NewGuid(), "山田太郎", "ヤマダタロウ", "東京都新宿区", null,
            "090-1234-5678", "taro@example.com", "taro01", "hashed-password", DateTime.Now);

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new LoginResponseAdapter();
    }

    [TestMethod(DisplayName = "ログイン実行結果をレスポンスへ変換する")]
    public void ToSource_ConvertsResultToResponse()
    {
        var customer = CreateCustomer();
        var token = new AccessToken("dummy.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        var result = new CustomerLoginResult(customer, token);

        var response = _adapter.ToSource(result);

        Assert.AreEqual("山田太郎", response.CustomerName);
        Assert.AreEqual("dummy.jwt.token", response.AccessToken);
    }

    [TestMethod(DisplayName = "ToStringにアクセストークンを含めない")]
    public void ToString_DoesNotExposeAccessToken()
    {
        var customer = CreateCustomer();
        var token = new AccessToken("secret.jwt.token", DateTimeOffset.UtcNow.AddMinutes(30));
        var result = new CustomerLoginResult(customer, token);

        var response = _adapter.ToSource(result);
        var text = response.ToString();

        Assert.DoesNotContain("secret.jwt.token", text);
        Assert.Contains("山田太郎", text);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new LoginResponse("山田太郎", "dummy.jwt.token");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}