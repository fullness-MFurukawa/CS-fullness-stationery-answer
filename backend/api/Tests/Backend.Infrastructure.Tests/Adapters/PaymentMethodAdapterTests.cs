using Backend.Infrastructure.Adapters;

using DomainPaymentMethod = Backend.Domain.Models.PaymentMethod;
using EfPaymentMethod = Backend.Infrastructure.Entities.PaymentMethod;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class PaymentMethodAdapterTests
{
    private readonly PaymentMethodAdapter _adapter = new();

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var source = new EfPaymentMethod
        {
            Id = 1,
            Name = "現金"
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(1, domain.Id);
        Assert.AreEqual("現金", domain.Name);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var domain = new DomainPaymentMethod(3, "銀行振込");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(3, source.Id);
        Assert.AreEqual("銀行振込", source.Name);
    }

    [TestMethod(DisplayName = "マスタなのでToSourceで主キーを設定する")]
    public void ToSource_SetsIdBecauseIdIsIdentity()
    {
        var domain = new DomainPaymentMethod(2, "クレジットカード");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(domain.Id, source.Id);
    }

    [TestMethod(DisplayName = "変換を往復してもIDと支払い方法名が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var original = new DomainPaymentMethod(1, "現金");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
    }
}