using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class EmployeeTests
{
    /// <summary>
    /// テスト用の部署を生成する
    /// </summary>
    private static Department CreateDepartment()
        => new(Guid.NewGuid(), "営業部");

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var department = CreateDepartment();

        var employee = new Employee(id, "フルネス太郎", "フルネスタロウ", department);

        Assert.AreEqual(id, employee.Id);
        Assert.AreEqual("フルネス太郎", employee.Name);
        Assert.AreEqual("フルネスタロウ", employee.NameKana);
        Assert.AreEqual(department, employee.Department);
    }

    [TestMethod(DisplayName = "社員名カナは未指定でも生成できる")]
    public void Constructor_NullNameKana_IsAllowed()
    {
        var employee = new Employee(Guid.NewGuid(), "フルネス太郎", null, CreateDepartment());

        Assert.IsNull(employee.NameKana);
    }

    [TestMethod(DisplayName = "社員名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        var department = CreateDepartment();

        Assert.ThrowsExactly<DomainException>(() => new Employee(Guid.NewGuid(), null!, "フルネスタロウ", department));
        Assert.ThrowsExactly<DomainException>(() => new Employee(Guid.NewGuid(), "", "フルネスタロウ", department));
        Assert.ThrowsExactly<DomainException>(() => new Employee(Guid.NewGuid(), "   ", "フルネスタロウ", department));
    }

    [TestMethod(DisplayName = "所属部署が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullDepartment_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Employee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", null!));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Employee(Guid.Empty, "フルネス太郎", "フルネスタロウ", CreateDepartment()));
    }
}