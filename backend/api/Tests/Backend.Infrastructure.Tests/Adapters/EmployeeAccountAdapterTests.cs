using Backend.Infrastructure.Adapters;

using DomainDepartment = Backend.Domain.Models.Department;
using DomainEmployee = Backend.Domain.Models.Employee;
using DomainEmployeeAccount = Backend.Domain.Models.EmployeeAccount;
using EfEmployeeAccount = Backend.Infrastructure.Entities.EmployeeAccount;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class EmployeeAccountAdapterTests
{
    private readonly EmployeeAccountAdapter _adapter = new();

    /// <summary>
    /// テスト用の社員（ドメイン）を生成する
    /// </summary>
    private static DomainEmployee CreateEmployee()
        => new(Guid.NewGuid(), "フルネス太郎", "フルネスタロウ", new DomainDepartment(Guid.NewGuid(), "販売管理部"));

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfEmployeeAccount CreateEntity(Guid uuid)
        => new()
        {
            Id = 1,
            AccountUuid = uuid,
            Name = "fullness",
            Password = "AQAAAAEAAYagAAAAEP7skW1gcyhy...",
            EmployeeId = 1
        };

    [TestMethod(DisplayName = "EFエンティティと変換済みの社員からドメインエンティティを生成できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var employee = CreateEmployee();
        var source = CreateEntity(uuid);

        var domain = _adapter.ToDomain(source, employee);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("fullness", domain.Name);
        Assert.AreEqual("AQAAAAEAAYagAAAAEP7skW1gcyhy...", domain.Password);
        Assert.AreEqual(employee, domain.Employee);
    }

    [TestMethod(DisplayName = "パスワードのハッシュ値は加工されずそのまま変換される")]
    public void ToDomain_PasswordHash_IsPassedThrough()
    {
        const string hash = "AQAAAAEAAYagAAAAEDWHnUvoQFQDSOJrgQyxH7Tt3sXRqOt2qtfqhj9r5aiYvaRqq/QHmyjV8U5HDUlMVg==";
        var source = CreateEntity(Guid.NewGuid());
        source.Password = hash;

        var domain = _adapter.ToDomain(source, CreateEmployee());

        Assert.AreEqual(hash, domain.Password);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainEmployeeAccount(uuid, "fullness", "hashed-password", CreateEmployee());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.AccountUuid);
        Assert.AreEqual("fullness", source.Name);
        Assert.AreEqual("hashed-password", source.Password);
    }

    [TestMethod(DisplayName = "ToSourceでは主キーと外部キーを設定しない")]
    public void ToSource_DoesNotSetKeys()
    {
        var domain = new DomainEmployeeAccount(Guid.NewGuid(), "fullness", "hashed-password", CreateEmployee());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
        Assert.AreEqual(0, source.EmployeeId);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var employee = CreateEmployee();
        var original = new DomainEmployeeAccount(uuid, "fullness", "hashed-password", employee);

        var restored = _adapter.ToDomain(_adapter.ToSource(original), employee);

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.Password, restored.Password);
        Assert.AreEqual(original.Employee, restored.Employee);
    }
}