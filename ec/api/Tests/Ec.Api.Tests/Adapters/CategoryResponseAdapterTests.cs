using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Domain.Models;
namespace Ec.Api.Tests.Adapters;

[TestClass]
[TestCategory("Ec.Api.Adapters")]
public class CategoryResponseAdapterTests
{
    private CategoryResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new CategoryResponseAdapter();
    }

    [TestMethod(DisplayName = "商品カテゴリをレスポンスへ変換する")]
    public void ToSource_ConvertsCategoryToResponse()
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");

        var response = _adapter.ToSource(category);

        Assert.AreEqual(category.Id, response.CategoryId);
        Assert.AreEqual("文房具", response.Name);
    }

    [TestMethod(DisplayName = "レスポンスからの逆変換はNotSupportedExceptionをスローする")]
    public void ToDomain_ThrowsNotSupportedException()
    {
        var response = new CategoryResponse(Guid.NewGuid(), "文房具");

        Assert.ThrowsExactly<NotSupportedException>(() => _adapter.ToDomain(response));
    }
}