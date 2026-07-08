using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class OrderStatusTests
{
    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var status = new OrderStatus(1, "注文済");

        Assert.AreEqual(1, status.Id);
        Assert.AreEqual("注文済", status.Name);
    }

    [TestMethod(DisplayName = "ステータス名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new OrderStatus(1, null!));
        Assert.ThrowsExactly<DomainException>(() => new OrderStatus(1, ""));
        Assert.ThrowsExactly<DomainException>(() => new OrderStatus(1, "   "));
    }

    [TestMethod(DisplayName = "IDが0ならDomainExceptionをスローする")]
    public void Constructor_ZeroId_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new OrderStatus(0, "注文済"));
    }
}