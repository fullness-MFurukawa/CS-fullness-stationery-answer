using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class ProductStockTests
{
    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();

        var stock = new ProductStock(id, 10);

        Assert.AreEqual(id, stock.Id);
        Assert.AreEqual(10, stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫数が0でも生成できる")]
    public void Constructor_ZeroQuantity_IsAllowed()
    {
        var stock = new ProductStock(Guid.NewGuid(), 0);

        Assert.AreEqual(0, stock.Quantity);
    }

    [TestMethod(DisplayName = "在庫数が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativeQuantity_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new ProductStock(Guid.NewGuid(), -1));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new ProductStock(Guid.Empty, 10));
    }
}