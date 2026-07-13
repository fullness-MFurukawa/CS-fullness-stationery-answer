using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
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

    [TestMethod(DisplayName = "社員アカウントをログインレスポンスへ変換する")]
    public void ToSource_ConvertsAccountToResponse()
    {
        var department = new Department(Guid.NewGuid(), "販売管理部");
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", department);
        var account = new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed-password", employee);

        var response = _adapter.ToSource(account);

        Assert.AreEqual("fullness", response.AccountName);
        Assert.AreEqual("フルネス太郎", response.EmployeeName);
    }

    [TestMethod(DisplayName = "変換後のレスポンスにパスワードハッシュが含まれない")]
    public void ToSource_DoesNotExposePassword()
    {
        var department = new Department(Guid.NewGuid(), "販売管理部");
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", department);
        var account = new EmployeeAccount(Guid.NewGuid(), "fullness", "secret-hash", employee);

        var response = _adapter.ToSource(account);

        Assert.AreNotEqual("secret-hash", response.AccountName);
        Assert.AreNotEqual("secret-hash", response.EmployeeName);
    }

    [TestMethod(DisplayName = "レスポンスからドメインオブジェクトへの変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new LoginResponse("fullness", "フルネス太郎");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}