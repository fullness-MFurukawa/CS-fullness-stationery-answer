using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class OrderDetailTests
{
    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(int price = 120)
        => new(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            price,
            null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var product = CreateProduct(120);

        var detail = new OrderDetail(1, product, 2);

        Assert.AreEqual(1, detail.Id);
        Assert.AreEqual(product, detail.Product);
        Assert.AreEqual(2, detail.Count);
    }

    [TestMethod(DisplayName = "小計は単価と注文数の積になる")]
    public void Subtotal_ReturnsPriceTimesCount()
    {
        var product = CreateProduct(120);

        var detail = new OrderDetail(1, product, 3);

        Assert.AreEqual(360, detail.Subtotal);
    }

    [TestMethod(DisplayName = "商品が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullProduct_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, null!, 1));
    }

    [TestMethod(DisplayName = "注文数が1未満ならDomainExceptionをスローする")]
    public void Constructor_CountLessThanOne_ThrowsDomainException()
    {
        var product = CreateProduct();

        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, product, 0));
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(1, product, -1));
    }

    [TestMethod(DisplayName = "IDが0ならDomainExceptionをスローする")]
    public void Constructor_ZeroId_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new OrderDetail(0, CreateProduct(), 1));
    }
}