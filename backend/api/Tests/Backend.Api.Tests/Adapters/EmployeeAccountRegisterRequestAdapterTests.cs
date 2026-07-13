using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class EmployeeAccountRegisterRequestAdapterTests
{
    private EmployeeAccountRegisterRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new EmployeeAccountRegisterRequestAdapter();
    }

    [TestMethod(DisplayName = "リクエストを入力値へ変換する")]
    public void ToDomain_ConvertsRequestToParam()
    {
        var employeeId = Guid.NewGuid();
        var request = new EmployeeAccountRegisterRequest(employeeId, "hanako01", "Password123");

        var param = _adapter.ToDomain(request);

        Assert.AreEqual(employeeId, param.EmployeeId);
        Assert.AreEqual("hanako01", param.AccountName);
        Assert.AreEqual("Password123", param.Password);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new EmployeeAccountRegisterParam(Guid.NewGuid(), "hanako01", "Password123");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}