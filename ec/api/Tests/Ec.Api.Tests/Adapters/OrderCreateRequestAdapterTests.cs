using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class OrderCreateRequestAdapterTests
{
    private OrderCreateRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderCreateRequestAdapter();
    }

    [TestMethod(DisplayName = "リクエストと顧客IDを入力値へ変換する")]
    public void ToParam_ConvertsRequestAndCustomerId()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var request = new OrderCreateRequest(1, [new OrderItemRequest(productId, 3)]);

        var param = _adapter.ToParam(request, customerId);

        // 顧客IDは引数で受け取ったものが使われる（リクエストには含まれない）
        Assert.AreEqual(customerId, param.CustomerId);
        Assert.AreEqual(1, param.PaymentMethodId);
        Assert.HasCount(1, param.Items);
        Assert.AreEqual(productId, param.Items[0].ProductId);
        Assert.AreEqual(3, param.Items[0].Count);
    }

    [TestMethod(DisplayName = "複数の注文明細を変換できる")]
    public void ToParam_MultipleItems()
    {
        var request = new OrderCreateRequest(1, [
            new OrderItemRequest(Guid.NewGuid(), 1),
            new OrderItemRequest(Guid.NewGuid(), 2),
        ]);

        var param = _adapter.ToParam(request, Guid.NewGuid());

        Assert.HasCount(2, param.Items);
    }
}