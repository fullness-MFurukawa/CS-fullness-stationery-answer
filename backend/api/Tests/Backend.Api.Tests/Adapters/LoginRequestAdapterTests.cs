using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
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
        var request = new LoginRequest("fullness", "Password123");

        var param = _adapter.ToDomain(request);

        Assert.AreEqual("fullness", param.AccountName);
        Assert.AreEqual("Password123", param.Password);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new EmployeeLoginParam("fullness", "Password123");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}