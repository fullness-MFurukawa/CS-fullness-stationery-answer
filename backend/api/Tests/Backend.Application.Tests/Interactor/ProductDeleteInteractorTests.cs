using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Tests.Fakes;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class ProductDeleteInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private ProductDeleteInteractor _interactor = null!;

    private ProductCategory _category = null!;
    private Product _existing = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _category = new ProductCategory(Guid.NewGuid(), "文房具");
        _existing = new Product(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            120,
            "https://example.com/pen.png",
            _category,
            new ProductStock(Guid.NewGuid(), 10));

        _productRepository = new Mock<IProductRepository>();
        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_existing);
        _productRepository
            .Setup(r => r.DeleteByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _interactor = new ProductDeleteInteractor(
            _productRepository.Object,
            new PassThroughUnitOfWork());
    }

    [TestMethod(DisplayName = "商品を論理削除しリポジトリの削除処理を呼び出す")]
    public async Task ExecuteAsync_ExistingProduct_CallsDelete()
    {
        await _interactor.ExecuteAsync(_existing.Id);

        _productRepository.Verify(r => r.DeleteByIdAsync(_existing.Id), Times.Once);
    }

    [TestMethod(DisplayName = "削除後の状態を反映した商品を返す")]
    public async Task ExecuteAsync_ExistingProduct_ReturnsDeletedProduct()
    {
        var product = await _interactor.ExecuteAsync(_existing.Id);

        Assert.IsTrue(product.IsDeleted);
        Assert.AreEqual(_existing.Id, product.Id);
        Assert.AreEqual("水性ボールペン(黒)", product.Name);
    }

    [TestMethod(DisplayName = "削除後も商品の他の項目は保持される")]
    public async Task ExecuteAsync_ExistingProduct_PreservesOtherValues()
    {
        var product = await _interactor.ExecuteAsync(_existing.Id);

        Assert.AreEqual(120, product.Price);
        Assert.AreEqual("https://example.com/pen.png", product.ImageUrl);
        Assert.AreEqual(_category, product.Category);
        Assert.AreEqual(_existing.Stock.Id, product.Stock.Id);
        Assert.AreEqual(10, product.Stock.Quantity);
    }

    [TestMethod(DisplayName = "商品が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_ProductNotFound_ThrowsNotFoundException()
    {
        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(Guid.NewGuid()));

        _productRepository.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod(DisplayName = "既に論理削除済みの商品はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_AlreadyDeletedProduct_ThrowsNotFoundException()
    {
        var deleted = new Product(
            _existing.Id,
            _existing.Name,
            _existing.Price,
            _existing.ImageUrl,
            _category,
            _existing.Stock,
            isDeleted: true);

        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(deleted);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(_existing.Id));

        _productRepository.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}