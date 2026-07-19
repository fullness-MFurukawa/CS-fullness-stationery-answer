using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class OrderDetailResponseAdapterTests
{
    private OrderDetailResponseAdapter _adapter = null!;

    private static OrderDetail CreateDetail()
        => new(
            new Product(
                Guid.NewGuid(), "水性ボールペン(黒)", 120, null,
                new ProductCategory(Guid.NewGuid(), "文房具"),
                new ProductStock(Guid.NewGuid(), 10)),
            3);

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new OrderDetailResponseAdapter();
    }

    [TestMethod(DisplayName = "注文明細をレスポンスへ変換する")]
    public void ToSource_ConvertsDetailToResponse()
    {
        var detail = CreateDetail();

        var response = _adapter.ToSource(detail);

        Assert.AreEqual(detail.Product.Id, response.ProductId);
        Assert.AreEqual("水性ボールペン(黒)", response.ProductName);
        Assert.AreEqual(120, response.Price);
        Assert.AreEqual(3, response.Count);
        Assert.AreEqual(360, response.Subtotal);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new OrderDetailResponse(Guid.NewGuid(), "商品", 100, 1, 100);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}