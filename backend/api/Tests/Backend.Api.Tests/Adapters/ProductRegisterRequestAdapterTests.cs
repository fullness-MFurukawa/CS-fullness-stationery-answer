using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;

using Microsoft.AspNetCore.Http;

using Moq;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class ProductRegisterRequestAdapterTests
{
    private ProductRegisterRequestAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new ProductRegisterRequestAdapter();
    }

    [TestMethod(DisplayName = "画像なしのリクエストを入力値へ変換する")]
    public void ToDomain_WithoutImage_ConvertsRequestToParam()
    {
        var categoryId = Guid.NewGuid();
        var request = new ProductRegisterRequest("テスト商品", 500, categoryId, 20, null);

        var param = _adapter.ToDomain(request);

        Assert.AreEqual("テスト商品", param.Name);
        Assert.AreEqual(500, param.Price);
        Assert.AreEqual(categoryId, param.CategoryId);
        Assert.AreEqual(20, param.Quantity);
        Assert.IsNull(param.ImageContent);
        Assert.AreEqual(0, param.ImageLength);
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

        var request = new ProductRegisterRequest("テスト商品", 500, Guid.NewGuid(), 20, image.Object);

        var param = _adapter.ToDomain(request);

        Assert.IsNotNull(param.ImageContent);
        Assert.AreEqual("pen.png", param.ImageFileName);
        Assert.AreEqual("image/png", param.ImageContentType);
        Assert.AreEqual(4, param.ImageLength);
    }

    [TestMethod(DisplayName = "入力値からリクエストへの変換はNotSupportedExceptionをスローする")]
    public void ToSource_ThrowsNotSupportedException()
    {
        var param = new Backend.Application.Params.ProductRegisterParam("テスト商品", 500, Guid.NewGuid(), 20);

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToSource(param));
    }
}