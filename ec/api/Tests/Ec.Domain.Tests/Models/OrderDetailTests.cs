using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class OrderDetailTests
{
    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(string name = "水性ボールペン(黒)", int price = 150)
        => new(
            Guid.NewGuid(),
            name,
            price,
            null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var product = CreateProduct();

        var detail = new OrderDetail(1, product, 3);

        Assert.AreEqual(1, detail.Id);
        Assert.AreEqual(product, detail.Product);
        Assert.AreEqual(3, detail.Count);
    }

    [TestMethod(DisplayName = "小計は単価と注文数の積になる")]
    public void Subtotal_ReturnsPriceTimesCount()
    {
        var product = CreateProduct(price: 150);

        var detail = new OrderDetail(1, product, 3);

        Assert.AreEqual(450, detail.Subtotal);
    }

    [TestMethod(DisplayName = "識別子を指定せずに生成できる（採番前）")]
    public void Constructor_WithoutId_IsNotPersisted()
    {
        var product = CreateProduct();

        var detail = new OrderDetail(product, 3);

        // 識別IDはデータベースの採番に委ねるため、この時点では未設定
        Assert.AreEqual(0, detail.Id);
        Assert.IsFalse(detail.IsPersisted);
        Assert.AreEqual(product, detail.Product);
        Assert.AreEqual(3, detail.Count);
    }

    [TestMethod(DisplayName = "識別子を指定して生成すると永続化済みとなる")]
    public void Constructor_WithId_IsPersisted()
    {
        var detail = new OrderDetail(1, CreateProduct(), 3);

        Assert.IsTrue(detail.IsPersisted);
    }

    [TestMethod(DisplayName = "商品が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingProduct_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, null!, 3));
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(null!, 3));
    }

    [TestMethod(DisplayName = "注文数が0以下ならDomainExceptionをスローする")]
    public void Constructor_NonPositiveCount_ThrowsDomainException()
    {
        var product = CreateProduct();

        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, product, 0));
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, product, -1));
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(product, 0));
    }

    [TestMethod(DisplayName = "採番済みの注文明細は識別子で等価判定される")]
    public void Equals_PersistedDetails_ComparedById()
    {
        var detail1 = new OrderDetail(1, CreateProduct(name: "商品A"), 3);
        var detail2 = new OrderDetail(1, CreateProduct(name: "商品B"), 5);

        // Entity<TId>は識別子のみで等価判定するため、中身が違っても等しい
        Assert.AreEqual(detail1, detail2);
    }

    [TestMethod(DisplayName = "採番前の注文明細は中身が違っても等価と判定される")]
    public void Equals_UnpersistedDetails_AreAllEqual()
    {
        var detail1 = new OrderDetail(CreateProduct(name: "商品A"), 3);
        var detail2 = new OrderDetail(CreateProduct(name: "商品B"), 5);

        // 識別子がどちらも0のため等しいと判定される。
        // 注文を生成してすぐ保存する今回の使い方では問題にならないが、
        // 採番前のエンティティをコレクションで比較する場合は注意が要る。
        // この挙動を把握しておくためにテストとして残す
        Assert.AreEqual(detail1, detail2);
    }
}