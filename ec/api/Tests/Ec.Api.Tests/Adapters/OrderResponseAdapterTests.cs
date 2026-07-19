using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class OrderResponseAdapterTests
{
    private OrderResponseAdapter _adapter = null!;

    private static Order CreateOrder()
    {
        var customer = new Customer(
            Guid.NewGuid(), "山田太郎", "ヤマダタロウ", "東京都新宿区", null,
            "090-1234-5678", "taro@example.com", "taro01", "hashed", DateTime.Now);
        var product = new Product(
            Guid.NewGuid(), "ボールペン", 120, null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));
        return Order.Create(
            Guid.NewGuid(),
            customer,
            new OrderStatus(OrderStatus.OrderedId, "注文済"),
            new PaymentMethod(1, "現金"),
            [new OrderDetail(product, 2)],
            DateTime.Now);
    }

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderResponseAdapter(new OrderDetailResponseAdapter());
    }

    [TestMethod(DisplayName = "注文をレスポンスへ変換する（明細を含む）")]
    public void ToSource_ConvertsOrderToResponse()
    {
        var order = CreateOrder();

        var response = _adapter.ToSource(order);

        Assert.AreEqual(order.Id, response.OrderId);
        Assert.AreEqual(240, response.AmountTotal);
        Assert.AreEqual("注文済", response.StatusName);
        Assert.AreEqual("現金", response.PaymentMethodName);
        Assert.HasCount(1, response.Details);
        Assert.AreEqual("ボールペン", response.Details[0].ProductName);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new OrderResponse(
            Guid.NewGuid(), DateTime.Now, 100, "注文済", "現金", []);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}