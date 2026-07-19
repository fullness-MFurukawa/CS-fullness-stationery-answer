using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
using Ec.Application.Params;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class CustomerRegisterRequestAdapterTests
{
    private CustomerRegisterRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new CustomerRegisterRequestAdapter();
    }

    [TestMethod(DisplayName = "リクエストを入力値へ変換する")]
    public void ToDomain_ConvertsRequestToParam()
    {
        var request = new CustomerRegisterRequest(
            "鈴木花子", "スズキハナコ", "東京都渋谷区", "渋谷ビル202",
            "090-1111-2222", "hanako@example.com", "hanako01", "pass12345");

        var param = _adapter.ToDomain(request);

        Assert.AreEqual("鈴木花子", param.Name);
        Assert.AreEqual("スズキハナコ", param.NameKana);
        Assert.AreEqual("東京都渋谷区", param.Address1);
        Assert.AreEqual("渋谷ビル202", param.Address2);
        Assert.AreEqual("090-1111-2222", param.PhoneNumber);
        Assert.AreEqual("hanako@example.com", param.MailAddress);
        Assert.AreEqual("hanako01", param.Username);
        Assert.AreEqual("pass12345", param.Password);
    }

    [TestMethod(DisplayName = "住所2がnullでも変換できる")]
    public void ToDomain_NullAddress2_IsAllowed()
    {
        var request = new CustomerRegisterRequest(
            "鈴木花子", "スズキハナコ", "東京都渋谷区", null,
            "090-1111-2222", "hanako@example.com", "hanako01", "pass12345");

        var param = _adapter.ToDomain(request);

        Assert.IsNull(param.Address2);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの逆変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new CustomerRegisterParam(
            "鈴木花子", "スズキハナコ", "東京都渋谷区", null,
            "090-1111-2222", "hanako@example.com", "hanako01", "pass12345");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}