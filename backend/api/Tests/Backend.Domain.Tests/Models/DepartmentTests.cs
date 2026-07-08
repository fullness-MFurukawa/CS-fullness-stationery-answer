using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class DepartmentTests
{
    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();

        var department = new Department(id, "営業部");

        Assert.AreEqual(id, department.Id);
        Assert.AreEqual("営業部", department.Name);
    }

    [TestMethod(DisplayName = "部署名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Department(Guid.NewGuid(), null!));
        Assert.ThrowsExactly<DomainException>(() => new Department(Guid.NewGuid(), ""));
        Assert.ThrowsExactly<DomainException>(() => new Department(Guid.NewGuid(), "   "));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Department(Guid.Empty, "営業部"));
    }
}