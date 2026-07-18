using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class PaymentMethodTests
{
    /// <summary>
    /// テスト用の支払い方法を生成する（各項目は任意で上書き可能）
    /// </summary>
    private static PaymentMethod CreatePaymentMethod(int id = 1, string name = "現金")
        => new(id, name);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var paymentMethod = CreatePaymentMethod(id: 1, name: "現金");

        Assert.AreEqual(1, paymentMethod.Id);
        Assert.AreEqual("現金", paymentMethod.Name);
    }

    [TestMethod(DisplayName = "支払い方法名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreatePaymentMethod(name: null!));
        Assert.ThrowsExactly<DomainException>(() => CreatePaymentMethod(name: ""));
        Assert.ThrowsExactly<DomainException>(() => CreatePaymentMethod(name: "   "));
    }

    [TestMethod(DisplayName = "識別子が0ならDomainExceptionをスローする")]
    public void Constructor_ZeroId_ThrowsDomainException()
    {
        // int型の既定値は0であり、Entity<int>は既定値を弾く
        Assert.ThrowsExactly<DomainException>(() => CreatePaymentMethod(id: 0));
    }
}