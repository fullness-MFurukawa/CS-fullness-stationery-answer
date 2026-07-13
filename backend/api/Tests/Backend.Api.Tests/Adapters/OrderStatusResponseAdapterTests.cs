using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class OrderStatusResponseAdapterTests
{
    private OrderStatusResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderStatusResponseAdapter();
    }

    [TestMethod(DisplayName = "ドメインオブジェクトをレスポンスへ変換する")]
    public void ToSource_ConvertsDomainToResponse()
    {
        var status = new OrderStatus(3, "配送中");

        var response = _adapter.ToSource(status);

        Assert.AreEqual(3, response.OrderStatusId);
        Assert.AreEqual("配送中", response.Name);
    }

    [TestMethod(DisplayName = "レスポンスをドメインオブジェクトへ変換する")]
    public void ToDomain_ConvertsResponseToDomain()
    {
        var response = new OrderStatusResponse(2, "入金済");

        var status = _adapter.ToDomain(response);

        Assert.AreEqual(2, status.Id);
        Assert.AreEqual("入金済", status.Name);
    }

    [TestMethod(DisplayName = "往復変換で値が保持される")]
    public void RoundTrip_PreservesValues()
    {
        var original = new OrderStatus(4, "完了");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original.Id, restored.Id);
        Assert.AreEqual(original.Name, restored.Name);
    }
}