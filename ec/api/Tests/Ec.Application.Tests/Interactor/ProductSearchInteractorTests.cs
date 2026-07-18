using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Application.Params;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class ProductSearchInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private ProductSearchInteractor _interactor = null!;

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(string name, ProductCategory category)
        => new(
            Guid.NewGuid(),
            name,
            150,
            null,
            category,
            new ProductStock(Guid.NewGuid(), 10));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _productRepository = new Mock<IProductRepository>();
        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _interactor = new ProductSearchInteractor(
            _productRepository.Object,
            _productCategoryRepository.Object);
    }

    [TestMethod(DisplayName = "カテゴリ未指定なら全商品を返す")]
    public async Task ExecuteAsync_NoCategory_ReturnsAllProducts()
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var products = new List<Product>
        {
            CreateProduct("ボールペン", category),
            CreateProduct("ノート", category),
        };
        _productRepository
            .Setup(r => r.FindAllAsync())
            .ReturnsAsync(products);

        var result = await _interactor.ExecuteAsync(new ProductSearchParam(null));

        Assert.HasCount(2, result);
        _productRepository.Verify(r => r.FindAllAsync(), Times.Once);
        // カテゴリ未指定なので、カテゴリの実在確認もカテゴリ別検索も行わない
        _productCategoryRepository.Verify(r => r.FindByIdAsync(It.IsAny<Guid>()), Times.Never);
        _productRepository.Verify(r => r.FindByCategoryAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod(DisplayName = "カテゴリ指定ありならそのカテゴリの商品を返す")]
    public async Task ExecuteAsync_WithCategory_ReturnsProductsInCategory()
    {
        var categoryId = Guid.NewGuid();
        var category = new ProductCategory(categoryId, "文房具");
        var products = new List<Product> { CreateProduct("ボールペン", category) };

        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(categoryId))
            .ReturnsAsync(category);
        _productRepository
            .Setup(r => r.FindByCategoryAsync(categoryId))
            .ReturnsAsync(products);

        var result = await _interactor.ExecuteAsync(new ProductSearchParam(categoryId));

        Assert.HasCount(1, result);
        Assert.AreEqual("ボールペン", result[0].Name);
        _productRepository.Verify(r => r.FindByCategoryAsync(categoryId), Times.Once);
        _productRepository.Verify(r => r.FindAllAsync(), Times.Never);
    }

    [TestMethod(DisplayName = "指定カテゴリに商品がなければ空の一覧を返す")]
    public async Task ExecuteAsync_CategoryWithNoProducts_ReturnsEmpty()
    {
        var categoryId = Guid.NewGuid();
        var category = new ProductCategory(categoryId, "文房具");

        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(categoryId))
            .ReturnsAsync(category);
        _productRepository
            .Setup(r => r.FindByCategoryAsync(categoryId))
            .ReturnsAsync([]);

        var result = await _interactor.ExecuteAsync(new ProductSearchParam(categoryId));

        // カテゴリは存在するが商品が0件。これは404ではなく空の一覧
        Assert.IsEmpty(result);
    }

    [TestMethod(DisplayName = "指定カテゴリが存在しなければNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_CategoryNotFound_ThrowsNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(categoryId))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new ProductSearchParam(categoryId)));

        // カテゴリが存在しないので、商品の検索は行わない
        _productRepository.Verify(r => r.FindByCategoryAsync(It.IsAny<Guid>()), Times.Never);
    }
}