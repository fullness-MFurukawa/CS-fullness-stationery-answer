using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class ProductStockTests
{
    /// <summary>
    /// テスト用の商品在庫を生成する（各項目は任意で上書き可能）
    /// </summary>
    private static ProductStock CreateStock(Guid? id = null, int quantity = 10)
        => new(id ?? Guid.NewGuid(), quantity);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();

        var stock = CreateStock(id: id, quantity: 30);

        Assert.AreEqual(id, stock.Id);
        Assert.AreEqual(30, stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫数が0でも生成できる")]
    public void Constructor_ZeroQuantity_IsAllowed()
    {
        var stock = CreateStock(quantity: 0);

        Assert.AreEqual(0, stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫数が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativeQuantity_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateStock(quantity: -1));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateStock(id: Guid.Empty));
    }

    [TestMethod(DisplayName = "在庫数以下の数量は購入できる")]
    public void CanPurchase_WithinQuantity_ReturnsTrue()
    {
        var stock = CreateStock(quantity: 10);

        Assert.IsTrue(stock.CanPurchase(1));
        // 在庫数ちょうどは購入できる（境界値）
        Assert.IsTrue(stock.CanPurchase(10));
    }

    [TestMethod(DisplayName = "在庫数を上回る数量は購入できない")]
    public void CanPurchase_ExceedsQuantity_ReturnsFalse()
    {
        var stock = CreateStock(quantity: 10);

        // 在庫数を1つ上回る（境界値）
        Assert.IsFalse(stock.CanPurchase(11));
    }

    [TestMethod(DisplayName = "0以下の数量は購入できない")]
    public void CanPurchase_NonPositiveCount_ReturnsFalse()
    {
        var stock = CreateStock(quantity: 10);

        Assert.IsFalse(stock.CanPurchase(0));
        Assert.IsFalse(stock.CanPurchase(-1));
    }

    [TestMethod(DisplayName = "在庫数が0なら購入できない")]
    public void CanPurchase_ZeroQuantity_ReturnsFalse()
    {
        var stock = CreateStock(quantity: 0);

        Assert.IsFalse(stock.CanPurchase(1));
    }

    [TestMethod(DisplayName = "購入した数量分だけ在庫が減る")]
    public void Reduce_ValidCount_ReturnsReducedStock()
    {
        var stock = CreateStock(quantity: 10);

        var reduced = stock.Reduce(3);

        Assert.AreEqual(7, reduced.Quantity);
    }

    [TestMethod(DisplayName = "在庫数ちょうどを購入すると在庫が0になる")]
    public void Reduce_ExactQuantity_ReturnsZeroStock()
    {
        var stock = CreateStock(quantity: 10);

        var reduced = stock.Reduce(10);

        Assert.AreEqual(0, reduced.Quantity);
    }

    [TestMethod(DisplayName = "在庫を減らしても元のインスタンスは変化しない")]
    public void Reduce_DoesNotMutateOriginal()
    {
        var stock = CreateStock(quantity: 10);

        stock.Reduce(3);

        // 不変であることの確認。
        // 自身を書き換える実装にすると、処理の途中で在庫が変わり、
        // 「どの時点の在庫か」が追えなくなる
        Assert.AreEqual(10, stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫を減らしても識別子は引き継がれる")]
    public void Reduce_KeepsIdentity()
    {
        var id = Guid.NewGuid();
        var stock = CreateStock(id: id, quantity: 10);

        var reduced = stock.Reduce(3);

        // 新しいインスタンスだが、同じ在庫レコードを指す。
        // 識別子が変わってしまうと、更新すべき行がわからなくなる
        Assert.AreEqual(id, reduced.Id);
        Assert.AreEqual(stock, reduced);
    }

    [TestMethod(DisplayName = "在庫数を上回る数量ならDomainExceptionをスローする")]
    public void Reduce_ExceedsQuantity_ThrowsDomainException()
    {
        var stock = CreateStock(quantity: 10);

        // 在庫数を1つ上回る（境界値）
        Assert.ThrowsExactly<DomainException>(() => stock.Reduce(11));
    }

    [TestMethod(DisplayName = "在庫が0のとき購入するとDomainExceptionをスローする")]
    public void Reduce_ZeroQuantity_ThrowsDomainException()
    {
        var stock = CreateStock(quantity: 0);

        Assert.ThrowsExactly<DomainException>(() => stock.Reduce(1));
    }

    [TestMethod(DisplayName = "0以下の数量ならDomainExceptionをスローする")]
    public void Reduce_NonPositiveCount_ThrowsDomainException()
    {
        var stock = CreateStock(quantity: 10);

        Assert.ThrowsExactly<DomainException>(() => stock.Reduce(0));
        Assert.ThrowsExactly<DomainException>(() => stock.Reduce(-1));
    }
}