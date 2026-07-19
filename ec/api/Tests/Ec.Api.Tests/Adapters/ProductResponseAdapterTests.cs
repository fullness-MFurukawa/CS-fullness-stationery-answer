using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class ProductResponseAdapterTests
{
    private ProductResponseAdapter _adapter = null!;

    private static Product CreateProduct()
        => new(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, "https://example.com/pen.png",
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new ProductResponseAdapter();
    }

    [TestMethod(DisplayName = "商品をレスポンスへ変換する（在庫とカテゴリを平坦化する）")]
    public void ToSource_ConvertsProductToResponse()
    {
        var product = CreateProduct();

        var response = _adapter.ToSource(product);

        Assert.AreEqual(product.Id, response.ProductId);
        Assert.AreEqual("水性ボールペン(黒)", response.Name);
        Assert.AreEqual(120, response.Price);
        Assert.AreEqual("https://example.com/pen.png", response.ImageUrl);
        Assert.AreEqual(10, response.Quantity);
        Assert.AreEqual(product.Category.Id, response.CategoryId);
        Assert.AreEqual("文房具", response.CategoryName);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new ProductResponse(
            Guid.NewGuid(), "商品", 100, null, 5, Guid.NewGuid(), "文房具");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}