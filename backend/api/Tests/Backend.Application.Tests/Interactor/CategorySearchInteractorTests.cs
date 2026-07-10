using Backend.Application.Interactor;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
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

    [TestMethod(DisplayName = "すべての商品カテゴリを取得する")]
    public async Task ExecuteAsync_ReturnsAllCategories()
    {
        IReadOnlyList<ProductCategory> expected =
        [
            new(Guid.NewGuid(), "文房具"),
            new(Guid.NewGuid(), "パソコン周辺機器"),
            new(Guid.NewGuid(), "雑貨")
        ];
        _productCategoryRepository.Setup(r => r.FindAllAsync()).ReturnsAsync(expected);

        var categories = await _interactor.ExecuteAsync();

        Assert.HasCount(3, categories);
        Assert.AreEqual("文房具", categories[0].Name);
        Assert.AreEqual("雑貨", categories[2].Name);
    }

    [TestMethod(DisplayName = "リポジトリの取得処理を1回だけ呼び出す")]
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _productCategoryRepository.Setup(r => r.FindAllAsync()).ReturnsAsync([]);

        await _interactor.ExecuteAsync();

        _productCategoryRepository.Verify(r => r.FindAllAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "カテゴリが0件でも例外にせず空の一覧を返す")]
    public async Task ExecuteAsync_NoCategories_ReturnsEmptyList()
    {
        _productCategoryRepository.Setup(r => r.FindAllAsync()).ReturnsAsync([]);

        var categories = await _interactor.ExecuteAsync();

        Assert.HasCount(0, categories);
    }
}