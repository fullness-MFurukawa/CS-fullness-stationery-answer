using Ec.Domain.Exceptions;
using Ec.Infrastructure.Adapters;
using DomainCustomer = Ec.Domain.Models.Customer;
using EfCustomer = Ec.Infrastructure.Entities.Customer;
namespace Ec.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Infrastructure.Adapters")]
public class CustomerAdapterTests
{
    private readonly CustomerAdapter _adapter = new();

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfCustomer CreateEntity(Guid uuid, DateTime createdAt)
        => new()
        {
            Id = 1,
            CustomerUuid = uuid,
            Name = "テスト顧客",
            NameKana = "テストコキャク",
            Address1 = "東京都新宿区",
            Address2 = "テストビル101",
            PhoneNumber = "090-1234-5678",
            MailAddress = "test@example.com",
            Username = "testuser",
            Password = "hashed-password",
            CreatedAt = createdAt
        };

    /// <summary>
    /// テスト用のドメインエンティティを生成する
    /// </summary>
    private static DomainCustomer CreateDomain(Guid uuid, DateTime createdAt)
        => new(
            uuid,
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            "テストビル101",
            "090-1234-5678",
            "test@example.com",
            "testuser",
            "hashed-password",
            createdAt);

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0);
        var source = CreateEntity(uuid, createdAt);

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("テスト顧客", domain.Name);
        Assert.AreEqual("テストコキャク", domain.NameKana);
        Assert.AreEqual("東京都新宿区", domain.Address1);
        Assert.AreEqual("テストビル101", domain.Address2);
        Assert.AreEqual("090-1234-5678", domain.PhoneNumber);
        Assert.AreEqual("test@example.com", domain.MailAddress);
        Assert.AreEqual("testuser", domain.Username);
        Assert.AreEqual("hashed-password", domain.Password);
        Assert.AreEqual(createdAt, domain.CreatedAt);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0);
        var domain = CreateDomain(uuid, createdAt);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.CustomerUuid);
        Assert.AreEqual("テスト顧客", source.Name);
        Assert.AreEqual("テストコキャク", source.NameKana);
        Assert.AreEqual("東京都新宿区", source.Address1);
        Assert.AreEqual("テストビル101", source.Address2);
        Assert.AreEqual("090-1234-5678", source.PhoneNumber);
        Assert.AreEqual("test@example.com", source.MailAddress);
        Assert.AreEqual("testuser", source.Username);
        Assert.AreEqual("hashed-password", source.Password);
        Assert.AreEqual(createdAt, source.CreatedAt);
    }

    [TestMethod(DisplayName = "住所2がnullでも変換できる")]
    public void ToDomain_NullAddress2_ReturnsDomainEntity()
    {
        var source = CreateEntity(Guid.NewGuid(), DateTime.Now);
        source.Address2 = null;

        var domain = _adapter.ToDomain(source);

        Assert.IsNull(domain.Address2);
    }

    [TestMethod(DisplayName = "氏名カナがnullの場合はドメインの制約によりDomainExceptionをスローする")]
    public void ToDomain_NullNameKana_ThrowsDomainException()
    {
        // EC側では氏名カナを必須とするため、DBにnullがあると変換時に弾かれる。
        // EC側が登録する顧客は必ずカナを持つため、通常このデータは発生しない。
        // ToDomain内で source.NameKana! を渡しているため、ドメインのコンストラクタが検証する
        var source = CreateEntity(Guid.NewGuid(), DateTime.Now);
        source.NameKana = null;

        Assert.ThrowsExactly<DomainException>(() => _adapter.ToDomain(source));
    }

    [TestMethod(DisplayName = "ToSourceではDB採番の主キーを設定しない")]
    public void ToSource_DoesNotSetDatabaseGeneratedId()
    {
        var domain = CreateDomain(Guid.NewGuid(), DateTime.Now);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0);
        var original = CreateDomain(uuid, createdAt);

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Username, restored.Username);
        Assert.AreEqual(original.MailAddress, restored.MailAddress);
        Assert.AreEqual(original.CreatedAt, restored.CreatedAt);
    }
}