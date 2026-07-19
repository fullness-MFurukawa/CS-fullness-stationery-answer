using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class CustomerResponseAdapterTests
{
    private CustomerResponseAdapter _adapter = null!;

    private static Customer CreateCustomer()
        => new(
            Guid.NewGuid(), "鈴木花子", "スズキハナコ", "東京都渋谷区", "渋谷ビル202",
            "090-1111-2222", "hanako@example.com", "hanako01", "hashed-password", DateTime.Now);

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new CustomerResponseAdapter();
    }

    [TestMethod(DisplayName = "顧客をレスポンスへ変換する")]
    public void ToSource_ConvertsCustomerToResponse()
    {
        var customer = CreateCustomer();

        var response = _adapter.ToSource(customer);

        Assert.AreEqual(customer.Id, response.CustomerId);
        Assert.AreEqual("鈴木花子", response.Name);
        Assert.AreEqual("hanako@example.com", response.MailAddress);
        Assert.AreEqual("hanako01", response.Username);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new CustomerResponse(Guid.NewGuid(), "鈴木花子", "hanako@example.com", "hanako01");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}