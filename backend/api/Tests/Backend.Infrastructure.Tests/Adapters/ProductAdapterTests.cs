using Backend.Infrastructure.Adapters;
using EfProduct = Backend.Infrastructure.Entities.Product;
using DomainProduct = Backend.Domain.Models.Product;
using DomainProductCategory = Backend.Domain.Models.ProductCategory;
using DomainProductStock = Backend.Domain.Models.ProductStock;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class ProductAdapterTests
{
    private readonly ProductAdapter _adapter = new();

    /// <summary>
    /// テスト用の商品カテゴリ（ドメイン）を生成する
    /// </summary>
    private static DomainProductCategory CreateCategory()
        => new(Guid.NewGuid(), "文房具");

    /// <summary>
    /// テスト用の商品在庫（ドメイン）を生成する
    /// </summary>
    private static DomainProductStock CreateStock(int quantity = 10)
        => new(Guid.NewGuid(), quantity);

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfProduct CreateEntity(Guid uuid, int deleteFlg = 0, string? imageUrl = null)
        => new()
        {
            Id = 1,
            ProductUuid = uuid,
            Name = "水性ボールペン(黒)",
            Price = 120,
            ImageUrl = imageUrl,
            ProductCategoryId = 1,
            DeleteFlg = deleteFlg
        };

    [TestMethod(DisplayName = "EFエンティティと変換済みの関連からドメインエンティティを生成できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var category = CreateCategory();
        var stock = CreateStock(10);
        var source = CreateEntity(uuid);

        var domain = _adapter.ToDomain(source, category, stock);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("水性ボールペン(黒)", domain.Name);
        Assert.AreEqual(120, domain.Price);
        Assert.IsNull(domain.ImageUrl);
        Assert.AreEqual(category, domain.Category);
        Assert.AreEqual(stock, domain.Stock);
        Assert.IsFalse(domain.IsDeleted);
    }

    [TestMethod(DisplayName = "削除フラグが1ならIsDeletedがtrueになる")]
    public void ToDomain_DeleteFlgIsOne_IsDeletedIsTrue()
    {
        var source = CreateEntity(Guid.NewGuid(), deleteFlg: 1);

        var domain = _adapter.ToDomain(source, CreateCategory(), CreateStock());

        Assert.IsTrue(domain.IsDeleted);
    }

    [TestMethod(DisplayName = "削除フラグが0以外ならIsDeletedがtrueになる")]
    public void ToDomain_DeleteFlgIsNonZero_IsDeletedIsTrue()
    {
        var source = CreateEntity(Guid.NewGuid(), deleteFlg: 2);

        var domain = _adapter.ToDomain(source, CreateCategory(), CreateStock());

        Assert.IsTrue(domain.IsDeleted);
    }

    [TestMethod(DisplayName = "画像URLが設定されていれば変換される")]
    public void ToDomain_WithImageUrl_MapsImageUrl()
    {
        var source = CreateEntity(Guid.NewGuid(), imageUrl: "https://example.com/pen.png");

        var domain = _adapter.ToDomain(source, CreateCategory(), CreateStock());

        Assert.AreEqual("https://example.com/pen.png", domain.ImageUrl);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainProduct(uuid, "水性ボールペン(黒)", 120, null, CreateCategory(), CreateStock());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.ProductUuid);
        Assert.AreEqual("水性ボールペン(黒)", source.Name);
        Assert.AreEqual(120, source.Price);
        Assert.IsNull(source.ImageUrl);
        Assert.AreEqual(0, source.DeleteFlg);
    }

    [TestMethod(DisplayName = "IsDeletedがtrueならDeleteFlgが1になる")]
    public void ToSource_IsDeletedIsTrue_DeleteFlgIsOne()
    {
        var domain = new DomainProduct(
            Guid.NewGuid(), "廃番ボールペン", 110, null, CreateCategory(), CreateStock(), isDeleted: true);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(1, source.DeleteFlg);
    }

    [TestMethod(DisplayName = "ToSourceでは主キー・外部キー・在庫を設定しない")]
    public void ToSource_DoesNotSetKeysAndStock()
    {
        var domain = new DomainProduct(Guid.NewGuid(), "水性ボールペン(黒)", 120, null, CreateCategory(), CreateStock());

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
        Assert.AreEqual(0, source.ProductCategoryId);
        Assert.IsNull(source.Stock);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var category = CreateCategory();
        var stock = CreateStock(10);
        var original = new DomainProduct(
            uuid, "水性ボールペン(黒)", 120, "https://example.com/pen.png", category, stock, isDeleted: true);

        var restored = _adapter.ToDomain(_adapter.ToSource(original), category, stock);

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
        Assert.AreEqual(original.Price, restored.Price);
        Assert.AreEqual(original.ImageUrl, restored.ImageUrl);
        Assert.IsTrue(restored.IsDeleted);
    }
}