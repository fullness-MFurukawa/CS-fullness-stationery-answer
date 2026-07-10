using Backend.Infrastructure.Adapters;

using DomainOrderStatus = Backend.Domain.Models.OrderStatus;
using EfOrderStatus = Backend.Infrastructure.Entities.OrderStatus;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class OrderStatusAdapterTests
{
    private readonly OrderStatusAdapter _adapter = new();

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var source = new EfOrderStatus
        {
            Id = 1,
            Name = "注文済"
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(1, domain.Id);
        Assert.AreEqual("注文済", domain.Name);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var domain = new DomainOrderStatus(3, "配送中");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(3, source.Id);
        Assert.AreEqual("配送中", source.Name);
    }

    [TestMethod(DisplayName = "マスタなのでToSourceで主キーを設定する")]
    public void ToSource_SetsIdBecauseIdIsIdentity()
    {
        var domain = new DomainOrderStatus(4, "完了");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(domain.Id, source.Id);
    }

    [TestMethod(DisplayName = "変換を往復してもIDとステータス名が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var original = new DomainOrderStatus(2, "入金済");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
    }
}