using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class OrderDetailResponseAdapterTests
{
    private OrderDetailResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderDetailResponseAdapter();
    }

    [TestMethod(DisplayName = "注文明細をレスポンスへ変換し小計を算出する")]
    public void ToSource_ConvertsDetailAndComputesSubtotal()
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var product = new Product(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, null, category,
            new ProductStock(Guid.NewGuid(), 10));
        var detail = new OrderDetail(1, product, 3);

        var response = _adapter.ToSource(detail);

        Assert.AreEqual("水性ボールペン(黒)", response.ProductName);
        Assert.AreEqual(120, response.Price);
        Assert.AreEqual(3, response.Count);
        Assert.AreEqual(360, response.Subtotal);
    }

    [TestMethod(DisplayName = "レスポンスからドメインオブジェクトへの変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new OrderDetailResponse("水性ボールペン(黒)", 120, 3, 360);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}