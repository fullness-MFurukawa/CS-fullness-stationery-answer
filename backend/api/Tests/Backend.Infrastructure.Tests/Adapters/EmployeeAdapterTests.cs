using Backend.Infrastructure.Adapters;

using DomainDepartment = Backend.Domain.Models.Department;
using DomainEmployee = Backend.Domain.Models.Employee;
using EfEmployee = Backend.Infrastructure.Entities.Employee;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class EmployeeAdapterTests
{
    private readonly EmployeeAdapter _adapter = new();

    /// <summary>
    /// テスト用の部署（ドメイン）を生成する
    /// </summary>
    private static DomainDepartment CreateDepartment()
        => new(Guid.NewGuid(), "販売管理部");

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfEmployee CreateEntity(Guid uuid, string? nameKana = "フルネスタロウ")
        => new()
        {
            Id = 1,
            EmployeeUuid = uuid,
            Name = "フルネス太郎",
            NameKana = nameKana,
            DepartmentId = 2
        };

    [TestMethod(DisplayName = "EFエンティティと変換済みの部署からドメインエンティティを生成できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var department = CreateDepartment();
        var source = CreateEntity(uuid);

        var domain = _adapter.ToDomain(source, department);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("フルネス太郎", domain.Name);
        Assert.AreEqual("フルネスタロウ", domain.NameKana);
        Assert.AreEqual(department, domain.Department);
    }

    [TestMethod(DisplayName = "社員名カナがnullでも変換できる")]
    public void ToDomain_NullNameKana_ReturnsDomainEntity()
    {
        var source = CreateEntity(Guid.NewGuid(), nameKana: null);

        var domain = _adapter.ToDomain(source, CreateDepartment());

        Assert.IsNull(domain.NameKana);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainEmployee(uuid, "フルネス太郎", "フルネスタロウ", CreateDepartment());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.EmployeeUuid);
        Assert.AreEqual("フルネス太郎", source.Name);
        Assert.AreEqual("フルネスタロウ", source.NameKana);
    }

    [TestMethod(DisplayName = "ToSourceでは主キーと外部キーを設定しない")]
    public void ToSource_DoesNotSetKeys()
    {
        var domain = new DomainEmployee(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", CreateDepartment());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
        Assert.AreEqual(0, source.DepartmentId);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var department = CreateDepartment();
        var original = new DomainEmployee(uuid, "フルネス太郎", "フルネスタロウ", department);

        var restored = _adapter.ToDomain(_adapter.ToSource(original), department);

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.NameKana, restored.NameKana);
        Assert.AreEqual(original.Department, restored.Department);
    }
}