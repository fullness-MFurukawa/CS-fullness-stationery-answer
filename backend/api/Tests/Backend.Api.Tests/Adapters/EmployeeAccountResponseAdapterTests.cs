using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class EmployeeAccountResponseAdapterTests
{
    private EmployeeAccountResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new EmployeeAccountResponseAdapter();
    }

    [TestMethod(DisplayName = "ドメインオブジェクトをレスポンスへ変換する")]
    public void ToSource_ConvertsDomainToResponse()
    {
        var department = new Department(Guid.NewGuid(), "商品企画部");
        var employee = new Employee(Guid.NewGuid(), "鈴木花子", "スズキハナコ", department);
        var accountId = Guid.NewGuid();
        var account = new EmployeeAccount(accountId, "hanako01", "hashed-password", employee);

        var response = _adapter.ToSource(account);

        Assert.AreEqual(accountId, response.AccountId);
        Assert.AreEqual("hanako01", response.AccountName);
        Assert.AreEqual("鈴木花子", response.EmployeeName);
    }

    [TestMethod(DisplayName = "変換後のレスポンスにパスワードハッシュが含まれない")]
    public void ToSource_DoesNotExposePassword()
    {
        var department = new Department(Guid.NewGuid(), "商品企画部");
        var employee = new Employee(Guid.NewGuid(), "鈴木花子", "スズキハナコ", department);
        var account = new EmployeeAccount(Guid.NewGuid(), "hanako01", "secret-hash", employee);

        var response = _adapter.ToSource(account);

        // レスポンスの各文字列プロパティにパスワードハッシュが混入していないことを確認する
        Assert.AreNotEqual("secret-hash", response.AccountId.ToString());
        Assert.AreNotEqual("secret-hash", response.AccountName);
        Assert.AreNotEqual("secret-hash", response.EmployeeName);
    }

    [TestMethod(DisplayName = "レスポンスからドメインオブジェクトへの変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new EmployeeAccountResponse(Guid.NewGuid(), "hanako01", "鈴木花子");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}