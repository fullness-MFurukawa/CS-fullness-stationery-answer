using Ec.Application.Interactor;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class CategorySearchInteractorTests
{
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private CategorySearchInteractor _interactor = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _interactor = new CategorySearchInteractor(_productCategoryRepository.Object);
    }

    [TestMethod(DisplayName = "商品カテゴリの一覧を返す")]
    public async Task ExecuteAsync_ReturnsCategories()
    {
        var categories = new List<ProductCategory>
        {
            new(Guid.NewGuid(), "文房具"),
            new(Guid.NewGuid(), "雑貨"),
        };
        _productCategoryRepository
            .Setup(r => r.FindAllAsync())
            .ReturnsAsync(categories);

        var result = await _interactor.ExecuteAsync();

        Assert.HasCount(2, result);
        Assert.AreEqual("文房具", result[0].Name);
        _productCategoryRepository.Verify(r => r.FindAllAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "カテゴリが0件なら空の一覧を返す")]
    public async Task ExecuteAsync_NoCategories_ReturnsEmpty()
    {
        _productCategoryRepository
            .Setup(r => r.FindAllAsync())
            .ReturnsAsync([]);

        var result = await _interactor.ExecuteAsync();

        Assert.IsEmpty(result);
    }
}