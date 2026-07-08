using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class EmployeeAccountTests
{
    /// <summary>
    /// テスト用の社員を生成する
    /// </summary>
    private static Employee CreateEmployee()
        => new(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", new Department(Guid.NewGuid(), "営業部"));

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var employee = CreateEmployee();

        var account = new EmployeeAccount(id, "fullness", "hashed-password", employee);

        Assert.AreEqual(id, account.Id);
        Assert.AreEqual("fullness", account.Name);
        Assert.AreEqual("hashed-password", account.Password);
        Assert.AreEqual(employee, account.Employee);
    }

    [TestMethod(DisplayName = "アカウント名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        var employee = CreateEmployee();

        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), null!, "hashed", employee));
        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), "", "hashed", employee));
        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), "   ", "hashed", employee));
    }

    [TestMethod(DisplayName = "パスワードが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingPassword_ThrowsDomainException()
    {
        var employee = CreateEmployee();

        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), "fullness", null!, employee));
        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), "fullness", "", employee));
        Assert.ThrowsExactly<DomainException>(() => new EmployeeAccount(Guid.NewGuid(), "fullness", "   ", employee));
    }

    [TestMethod(DisplayName = "社員が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullEmployee_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed", null!));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new EmployeeAccount(Guid.Empty, "fullness", "hashed", CreateEmployee()));
    }
}