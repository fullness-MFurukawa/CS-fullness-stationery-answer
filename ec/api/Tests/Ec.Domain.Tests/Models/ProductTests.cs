using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class ProductTests
{
    /// <summary>
    /// テスト用の商品カテゴリを生成する
    /// </summary>
    private static ProductCategory CreateCategory(string name = "文房具")
        => new(Guid.NewGuid(), name);

    /// <summary>
    /// テスト用の商品在庫を生成する
    /// </summary>
    private static ProductStock CreateStock(int quantity = 10)
        => new(Guid.NewGuid(), quantity);

    /// <summary>
    /// テスト用の商品を生成する（各項目は任意で上書き可能）
    /// </summary>
    private static Product CreateProduct(
        Guid? id = null,
        string name = "水性ボールペン(黒)",
        int price = 150,
        string? imageUrl = "https://example.com/images/pen.png",
        ProductCategory? category = null,
        ProductStock? stock = null,
        bool isDeleted = false)
        => new(
            id ?? Guid.NewGuid(),
            name,
            price,
            imageUrl,
            category ?? CreateCategory(),
            stock ?? CreateStock(),
            isDeleted);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var category = CreateCategory();
        var stock = CreateStock(quantity: 30);

        var product = CreateProduct(id: id, category: category, stock: stock);

        Assert.AreEqual(id, product.Id);
        Assert.AreEqual("水性ボールペン(黒)", product.Name);
        Assert.AreEqual(150, product.Price);
        Assert.AreEqual("https://example.com/images/pen.png", product.ImageUrl);
        Assert.AreEqual(category, product.Category);
        Assert.AreEqual(stock, product.Stock);
        Assert.IsFalse(product.IsDeleted);
    }

    [TestMethod(DisplayName = "価格が0でも生成できる")]
    public void Constructor_ZeroPrice_IsAllowed()
    {
        var product = CreateProduct(price: 0);

        Assert.AreEqual(0, product.Price);
    }

    [TestMethod(DisplayName = "画像URLが未指定でも生成できる")]
    public void Constructor_NullImageUrl_IsAllowed()
    {
        var product = CreateProduct(imageUrl: null);

        Assert.IsNull(product.ImageUrl);
    }

    [TestMethod(DisplayName = "画像URLが空文字や空白のみならnullへ正規化される")]
    public void Constructor_BlankImageUrl_NormalizedToNull()
    {
        // 空文字とnullが混在すると「画像なし」の表現が2通りになるため、
        // nullへ寄せる
        Assert.IsNull(CreateProduct(imageUrl: "").ImageUrl);
        Assert.IsNull(CreateProduct(imageUrl: "   ").ImageUrl);
    }

    [TestMethod(DisplayName = "商品名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateProduct(name: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateProduct(name: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateProduct(name: "   "));
    }

    [TestMethod(DisplayName = "価格が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativePrice_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateProduct(price: -1));
    }


    [TestMethod(DisplayName = "商品カテゴリが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingCategory_ThrowsDomainException()
    {
        // ヘルパーは未指定の引数を既定値へ置き換えるため、
        // nullを渡す検証では使えない。コンストラクタを直接呼ぶ
        Assert.ThrowsExactly<DomainException>(() => new Product(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            150,
            null,
            null!,
            CreateStock()));
    }


    [TestMethod(DisplayName = "商品在庫が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingStock_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Product(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            150,
            null,
            CreateCategory(),
            null!));
    }



    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateProduct(id: Guid.Empty));
    }

    [TestMethod(DisplayName = "在庫がある商品は購入できる")]
    public void CanPurchase_InStock_ReturnsTrue()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        Assert.IsTrue(product.CanPurchase(1));
        Assert.IsTrue(product.CanPurchase(10));
    }

    [TestMethod(DisplayName = "在庫数を上回る数量は購入できない")]
    public void CanPurchase_ExceedsStock_ReturnsFalse()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        Assert.IsFalse(product.CanPurchase(11));
    }

    [TestMethod(DisplayName = "論理削除された商品は在庫があっても購入できない")]
    public void CanPurchase_DeletedProduct_ReturnsFalse()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10), isDeleted: true);

        // カートに入れた後に管理者が商品を削除した場合を想定する
        Assert.IsFalse(product.CanPurchase(1));
    }

    [TestMethod(DisplayName = "購入した数量分だけ在庫が減る")]
    public void ReduceStock_ValidCount_ReturnsProductWithReducedStock()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        var reduced = product.ReduceStock(3);

        Assert.AreEqual(7, reduced.Stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫を減らしても商品の他の情報は変わらない")]
    public void ReduceStock_KeepsOtherProperties()
    {
        var id = Guid.NewGuid();
        var category = CreateCategory();
        var product = CreateProduct(id: id, category: category, stock: CreateStock(quantity: 10));

        var reduced = product.ReduceStock(3);

        Assert.AreEqual(id, reduced.Id);
        Assert.AreEqual("水性ボールペン(黒)", reduced.Name);
        Assert.AreEqual(150, reduced.Price);
        Assert.AreEqual("https://example.com/images/pen.png", reduced.ImageUrl);
        Assert.AreEqual(category, reduced.Category);
        Assert.IsFalse(reduced.IsDeleted);
    }

    [TestMethod(DisplayName = "在庫を減らしても元のインスタンスは変化しない")]
    public void ReduceStock_DoesNotMutateOriginal()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        product.ReduceStock(3);

        Assert.AreEqual(10, product.Stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫数を上回る数量ならDomainExceptionをスローする")]
    public void ReduceStock_ExceedsStock_ThrowsDomainException()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        Assert.ThrowsExactly<DomainException>(() => product.ReduceStock(11));
    }

    [TestMethod(DisplayName = "0以下の数量ならDomainExceptionをスローする")]
    public void ReduceStock_NonPositiveCount_ThrowsDomainException()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10));

        Assert.ThrowsExactly<DomainException>(() => product.ReduceStock(0));
        Assert.ThrowsExactly<DomainException>(() => product.ReduceStock(-1));
    }

    [TestMethod(DisplayName = "論理削除された商品の在庫を減らすとDomainExceptionをスローする")]
    public void ReduceStock_DeletedProduct_ThrowsDomainException()
    {
        var product = CreateProduct(stock: CreateStock(quantity: 10), isDeleted: true);

        // 集約のルートである商品を経由することで、
        // 「販売終了した商品の在庫が減る」という不整合を防ぐ
        Assert.ThrowsExactly<DomainException>(() => product.ReduceStock(1));
    }
}