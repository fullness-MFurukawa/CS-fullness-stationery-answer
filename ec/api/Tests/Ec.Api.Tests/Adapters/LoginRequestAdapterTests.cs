using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
using Ec.Application.Params;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class LoginRequestAdapterTests
{
    private LoginRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new LoginRequestAdapter();
    }

    [TestMethod(DisplayName = "リクエストを入力値へ変換する")]
    public void ToDomain_ConvertsRequestToParam()
    {
        var request = new LoginRequest("taro@example.com", "Password123");

        var param = _adapter.ToDomain(request);

        Assert.AreEqual("taro@example.com", param.MailAddress);
        Assert.AreEqual("Password123", param.Password);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの逆変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new CustomerLoginParam("taro@example.com", "Password123");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}