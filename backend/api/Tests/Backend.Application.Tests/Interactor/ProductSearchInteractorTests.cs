using Backend.Application.Interactor;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class ProductSearchInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private ProductSearchInteractor _interactor = null!;

    private ProductCategory _category = null!;

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private Product CreateProduct(string name)
        => new(
            Guid.NewGuid(),
            name,
            120,
            null,
            _category,
            new ProductStock(Guid.NewGuid(), 10));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _category = new ProductCategory(Guid.NewGuid(), "文房具");

        _productRepository = new Mock<IProductRepository>();
        _interactor = new ProductSearchInteractor(_productRepository.Object);
    }

    [TestMethod(DisplayName = "カテゴリを指定しない場合は全件取得する")]
    public async Task ExecuteAsync_WithoutCategory_CallsFindAll()
    {
        IReadOnlyList<Product> expected = [CreateProduct("水性ボールペン(黒)"), CreateProduct("鉛筆(黒)")];
        _productRepository.Setup(r => r.FindAllAsync()).ReturnsAsync(expected);

        var products = await _interactor.ExecuteAsync(null);

        Assert.HasCount(2, products);
        _productRepository.Verify(r => r.FindAllAsync(), Times.Once);
        _productRepository.Verify(r => r.FindByCategoryAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod(DisplayName = "カテゴリを指定した場合はカテゴリで絞り込んで取得する")]
    public async Task ExecuteAsync_WithCategory_CallsFindByCategory()
    {
        IReadOnlyList<Product> expected = [CreateProduct("水性ボールペン(黒)")];
        _productRepository.Setup(r => r.FindByCategoryAsync(_category.Id)).ReturnsAsync(expected);

        var products = await _interactor.ExecuteAsync(_category.Id);

        Assert.HasCount(1, products);
        Assert.AreEqual("水性ボールペン(黒)", products[0].Name);
        _productRepository.Verify(r => r.FindByCategoryAsync(_category.Id), Times.Once);
        _productRepository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "該当する商品が0件でも例外にせず空の一覧を返す")]
    public async Task ExecuteAsync_NoMatch_ReturnsEmptyList()
    {
        _productRepository.Setup(r => r.FindByCategoryAsync(It.IsAny<Guid>()))
            .ReturnsAsync([]);

        var products = await _interactor.ExecuteAsync(_category.Id);

        Assert.HasCount(0, products);
    }
}