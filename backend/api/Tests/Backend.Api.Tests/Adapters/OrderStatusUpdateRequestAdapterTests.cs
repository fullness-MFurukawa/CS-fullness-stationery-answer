using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class OrderStatusUpdateRequestAdapterTests
{
    private OrderStatusUpdateRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderStatusUpdateRequestAdapter();
    }

    [TestMethod(DisplayName = "リクエストを入力値へ変換する")]
    public void ToDomain_ConvertsRequestToParam()
    {
        var orderId = Guid.NewGuid();
        var request = new OrderStatusUpdateRequest(2) { OrderId = orderId };

        var param = _adapter.ToDomain(request);

        Assert.AreEqual(orderId, param.OrderId);
        Assert.AreEqual(2, param.OrderStatusId);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new OrderStatusUpdateParam(Guid.NewGuid(), 2);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}