using Backend.Infrastructure.Adapters;
using EfOrderDetail = Backend.Infrastructure.Entities.OrderDetail;
using DomainOrderDetail = Backend.Domain.Models.OrderDetail;
using DomainProduct = Backend.Domain.Models.Product;
using DomainProductCategory = Backend.Domain.Models.ProductCategory;
using DomainProductStock = Backend.Domain.Models.ProductStock;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class OrderDetailAdapterTests
{
    private readonly OrderDetailAdapter _adapter = new();

    /// <summary>
    /// テスト用の商品（ドメイン）を生成する
    /// </summary>
    private static DomainProduct CreateProduct(int price = 120)
        => new(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            price,
            null,
            new DomainProductCategory(Guid.NewGuid(), "文房具"),
            new DomainProductStock(Guid.NewGuid(), 10));

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfOrderDetail CreateEntity(int id = 1, int count = 2)
        => new()
        {
            Id = id,
            OrderId = 5,
            ProductId = 3,
            Count = count
        };

    [TestMethod(DisplayName = "EFエンティティと変換済みの商品からドメインエンティティを生成できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var product = CreateProduct(120);
        var source = CreateEntity(id: 1, count: 2);

        var domain = _adapter.ToDomain(source, product);

        Assert.AreEqual(1, domain.Id);
        Assert.AreEqual(product, domain.Product);
        Assert.AreEqual(2, domain.Count);
    }

    [TestMethod(DisplayName = "小計は単価と注文数の積になる")]
    public void ToDomain_Subtotal_IsPriceTimesCount()
    {
        var product = CreateProduct(120);
        var source = CreateEntity(id: 1, count: 3);

        var domain = _adapter.ToDomain(source, product);

        Assert.AreEqual(360, domain.Subtotal);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var domain = new DomainOrderDetail(7, CreateProduct(), 4);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(7, source.Id);
        Assert.AreEqual(4, source.Count);
    }

    [TestMethod(DisplayName = "同一性がintのIDなのでToSourceで主キーを設定する")]
    public void ToSource_SetsIdBecauseIdIsIdentity()
    {
        var domain = new DomainOrderDetail(7, CreateProduct(), 1);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(domain.Id, source.Id);
    }

    [TestMethod(DisplayName = "ToSourceでは外部キーを設定しない")]
    public void ToSource_DoesNotSetForeignKeys()
    {
        var domain = new DomainOrderDetail(7, CreateProduct(), 1);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.OrderId);
        Assert.AreEqual(0, source.ProductId);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var product = CreateProduct(120);
        var original = new DomainOrderDetail(7, product, 2);

        var restored = _adapter.ToDomain(_adapter.ToSource(original), product);

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Count, restored.Count);
        Assert.AreEqual(original.Subtotal, restored.Subtotal);
    }
}