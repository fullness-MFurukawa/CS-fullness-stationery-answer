using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class PaymentMethodResponseAdapterTests
{
    private PaymentMethodResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new PaymentMethodResponseAdapter();
    }

    [TestMethod(DisplayName = "支払い方法をレスポンスへ変換する")]
    public void ToSource_ConvertsPaymentMethodToResponse()
    {
        var method = new PaymentMethod(1, "現金");

        var response = _adapter.ToSource(method);

        Assert.AreEqual(1, response.PaymentMethodId);
        Assert.AreEqual("現金", response.Name);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new PaymentMethodResponse(1, "現金");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}