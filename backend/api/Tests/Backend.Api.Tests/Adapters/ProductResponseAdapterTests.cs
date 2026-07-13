using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class ProductResponseAdapterTests
{
    private ProductResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new ProductResponseAdapter();
    }

    [TestMethod(DisplayName = "商品集約をレスポンスへ平坦化して変換する")]
    public void ToSource_FlattensAggregateToResponse()
    {
        var categoryId = Guid.NewGuid();
        var category = new ProductCategory(categoryId, "文房具");
        var productId = Guid.NewGuid();
        var product = new Product(
            productId, "水性ボールペン(黒)", 120, "https://example.com/pen.png",
            category, new ProductStock(Guid.NewGuid(), 10));

        var response = _adapter.ToSource(product);

        Assert.AreEqual(productId, response.ProductId);
        Assert.AreEqual("水性ボールペン(黒)", response.Name);
        Assert.AreEqual(120, response.Price);
        Assert.AreEqual("https://example.com/pen.png", response.ImageUrl);
        Assert.AreEqual(10, response.Quantity);
        Assert.AreEqual(categoryId, response.CategoryId);
        Assert.AreEqual("文房具", response.CategoryName);
        Assert.IsFalse(response.IsDeleted);
    }

    [TestMethod(DisplayName = "画像なしの商品はレスポンスの画像URLがnullになる")]
    public void ToSource_NoImage_ImageUrlIsNull()
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var product = new Product(
            Guid.NewGuid(), "鉛筆(黒)", 100, null,
            category, new ProductStock(Guid.NewGuid(), 200));

        var response = _adapter.ToSource(product);

        Assert.IsNull(response.ImageUrl);
    }

    [TestMethod(DisplayName = "レスポンスからドメインオブジェクトへの変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new ProductResponse(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, null, 10,
            Guid.NewGuid(), "文房具", false);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}