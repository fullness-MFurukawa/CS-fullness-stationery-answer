using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Domain.Models;

namespace Backend.Api.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Api.Adapters")]
public class CategoryResponseAdapterTests
{
    private CategoryResponseAdapter _adapter = null!;

    [TestInitialize]
    public void SetUp()
    {
        _adapter = new CategoryResponseAdapter();
    }

    [TestMethod(DisplayName = "ドメインオブジェクトをレスポンスへ変換する")]
    public void ToSource_ConvertsDomainToResponse()
    {
        var id = Guid.NewGuid();
        var category = new ProductCategory(id, "文房具");

        var response = _adapter.ToSource(category);

        Assert.AreEqual(id, response.CategoryId);
        Assert.AreEqual("文房具", response.Name);
    }

    [TestMethod(DisplayName = "レスポンスをドメインオブジェクトへ変換する")]
    public void ToDomain_ConvertsResponseToDomain()
    {
        var id = Guid.NewGuid();
        var response = new CategoryResponse(id, "雑貨");

        var category = _adapter.ToDomain(response);

        Assert.AreEqual(id, category.Id);
        Assert.AreEqual("雑貨", category.Name);
    }

    [TestMethod(DisplayName = "往復変換で値が保持される")]
    public void RoundTrip_PreservesValues()
    {
        var original = new ProductCategory(Guid.NewGuid(), "画材");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original.Id, restored.Id);
        Assert.AreEqual(original.Name, restored.Name);
    }
}