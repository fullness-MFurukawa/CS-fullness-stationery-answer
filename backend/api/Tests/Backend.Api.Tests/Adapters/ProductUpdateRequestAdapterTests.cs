using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class ProductUpdateRequestAdapterTests
{
    private ProductUpdateRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new ProductUpdateRequestAdapter();
    }

    [TestMethod(DisplayName = "画像なしのリクエストを入力値へ変換する")]
    public void ToDomain_WithoutImage_ConvertsRequestToParam()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var request = new ProductUpdateRequest("テスト商品 改", 550, categoryId, 30, null)
        {
            ProductId = productId,
        };

        var param = _adapter.ToDomain(request);

        Assert.AreEqual(productId, param.ProductId);
        Assert.AreEqual("テスト商品 改", param.Name);
        Assert.AreEqual(550, param.Price);
        Assert.AreEqual(categoryId, param.CategoryId);
        Assert.AreEqual(30, param.Quantity);
        Assert.IsNull(param.ImageContent);
    }

    [TestMethod(DisplayName = "画像ありのリクエストは画像情報を入力値へ引き渡す")]
    public void ToDomain_WithImage_PassesImageInfoToParam()
    {
        var content = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);
        var image = new Mock<IFormFile>();
        image.Setup(f => f.OpenReadStream()).Returns(content);
        image.Setup(f => f.FileName).Returns("pen.png");
        image.Setup(f => f.ContentType).Returns("image/png");
        image.Setup(f => f.Length).Returns(4);

        var request = new ProductUpdateRequest("テスト商品 改", 550, Guid.NewGuid(), 30, image.Object)
        {
            ProductId = Guid.NewGuid(),
        };

        var param = _adapter.ToDomain(request);

        Assert.IsNotNull(param.ImageContent);
        Assert.AreEqual("pen.png", param.ImageFileName);
        Assert.AreEqual("image/png", param.ImageContentType);
        Assert.AreEqual(4, param.ImageLength);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new Backend.Application.Params.ProductUpdateParam(
            Guid.NewGuid(), "テスト商品 改", 550, Guid.NewGuid(), 30);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}