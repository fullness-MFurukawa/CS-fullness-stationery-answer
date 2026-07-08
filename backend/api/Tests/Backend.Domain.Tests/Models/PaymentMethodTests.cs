using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class PaymentMethodTests
{
    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var paymentMethod = new PaymentMethod(1, "現金");

        Assert.AreEqual(1, paymentMethod.Id);
        Assert.AreEqual("現金", paymentMethod.Name);
    }

    [TestMethod(DisplayName = "支払い方法名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new PaymentMethod(1, null!));
        Assert.ThrowsExactly<DomainException>(() => new PaymentMethod(1, ""));
        Assert.ThrowsExactly<DomainException>(() => new PaymentMethod(1, "   "));
    }

    [TestMethod(DisplayName = "IDが0ならDomainExceptionをスローする")]
    public void Constructor_ZeroId_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new PaymentMethod(0, "現金"));
    }
}