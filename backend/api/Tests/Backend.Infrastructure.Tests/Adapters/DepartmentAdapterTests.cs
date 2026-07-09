using Backend.Infrastructure.Adapters;
using EfDepartment = Backend.Infrastructure.Entities.Department;
using DomainDepartment = Backend.Domain.Models.Department;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class DepartmentAdapterTests
{
    private readonly DepartmentAdapter _adapter = new();

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var source = new EfDepartment
        {
            Id = 1,
            DepartmentUuid = uuid,
            Name = "営業部"
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("営業部", domain.Name);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainDepartment(uuid, "営業部");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.DepartmentUuid);
        Assert.AreEqual("営業部", source.Name);
    }

    [TestMethod(DisplayName = "ToSourceではDB採番の主キーを設定しない")]
    public void ToSource_DoesNotSetDatabaseGeneratedId()
    {
        var domain = new DomainDepartment(Guid.NewGuid(), "営業部");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
    }

    [TestMethod(DisplayName = "変換を往復しても識別IDと部署名が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var original = new DomainDepartment(uuid, "営業部");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
    }
}