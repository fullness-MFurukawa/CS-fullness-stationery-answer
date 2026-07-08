using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class ProductTests
{
    /// <summary>
    /// テスト用の商品カテゴリを生成する
    /// </summary>
    private static ProductCategory CreateCategory()
        => new(Guid.NewGuid(), "文房具");

    /// <summary>
    /// テスト用の商品在庫を生成する
    /// </summary>
    private static ProductStock CreateStock(int quantity = 10)
        => new(Guid.NewGuid(), quantity);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var category = CreateCategory();
        var stock = CreateStock(10);

        var product = new Product(id, "水性ボールペン(黒)", 120, null, category, stock);

        Assert.AreEqual(id, product.Id);
        Assert.AreEqual("水性ボールペン(黒)", product.Name);
        Assert.AreEqual(120, product.Price);
        Assert.IsNull(product.ImageUrl);
        Assert.AreEqual(category, product.Category);
        Assert.AreEqual(stock, product.Stock);
        Assert.AreEqual(10, product.Stock.Quantity);
        Assert.IsFalse(product.IsDeleted);
    }

    [TestMethod(DisplayName = "論理削除フラグを指定して生成できる")]
    public void Constructor_IsDeletedTrue_SetsIsDeleted()
    {
        var product = new Product(
            Guid.NewGuid(), "廃番ボールペン", 110, null, CreateCategory(), CreateStock(), isDeleted: true);

        Assert.IsTrue(product.IsDeleted);
    }

    [TestMethod(DisplayName = "価格が0でも生成できる")]
    public void Constructor_ZeroPrice_IsAllowed()
    {
        var product = new Product(Guid.NewGuid(), "無料サンプル", 0, null, CreateCategory(), CreateStock());

        Assert.AreEqual(0, product.Price);
    }

    [TestMethod(DisplayName = "商品名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        var category = CreateCategory();
        var stock = CreateStock();

        Assert.ThrowsExactly<DomainException>(() => new Product(Guid.NewGuid(), null!, 120, null, category, stock));
        Assert.ThrowsExactly<DomainException>(() => new Product(Guid.NewGuid(), "", 120, null, category, stock));
        Assert.ThrowsExactly<DomainException>(() => new Product(Guid.NewGuid(), "   ", 120, null, category, stock));
    }

    [TestMethod(DisplayName = "価格が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativePrice_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Product(Guid.NewGuid(), "水性ボールペン(黒)", -1, null, CreateCategory(), CreateStock()));
    }

    [TestMethod(DisplayName = "商品カテゴリが未指定ならDomainExceptionをスローする")]
    public void Constructor_NullCategory_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Product(Guid.NewGuid(), "水性ボールペン(黒)", 120, null, null!, CreateStock()));
    }

    [TestMethod(DisplayName = "商品在庫が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullStock_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Product(Guid.NewGuid(), "水性ボールペン(黒)", 120, null, CreateCategory(), null!));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(
            () => new Product(Guid.Empty, "水性ボールペン(黒)", 120, null, CreateCategory(), CreateStock()));
    }
}