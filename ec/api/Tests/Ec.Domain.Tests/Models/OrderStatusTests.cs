using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class OrderStatusTests
{
    /// <summary>
    /// テスト用の注文ステータスを生成する（各項目は任意で上書き可能）
    /// </summary>
    private static OrderStatus CreateOrderStatus(int id = 1, string name = "注文済")
        => new(id, name);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var status = CreateOrderStatus(id: 1, name: "注文済");

        Assert.AreEqual(1, status.Id);
        Assert.AreEqual("注文済", status.Name);
    }

    [TestMethod(DisplayName = "注文ステータス名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateOrderStatus(name: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateOrderStatus(name: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateOrderStatus(name: "   "));
    }

    [TestMethod(DisplayName = "識別子が0ならDomainExceptionをスローする")]
    public void Constructor_ZeroId_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateOrderStatus(id: 0));
    }

}