using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Params;
using Backend.Application.Tests.Fakes;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class ProductUpdateInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private ProductUpdateInteractor _interactor = null!;

    private ProductCategory _oldCategory = null!;
    private ProductCategory _newCategory = null!;
    private Product _existing = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _oldCategory = new ProductCategory(Guid.NewGuid(), "文房具");
        _newCategory = new ProductCategory(Guid.NewGuid(), "雑貨");

        _existing = new Product(
            Guid.NewGuid(),
            "水性ボールペン(黒)",
            120,
            null,
            _oldCategory,
            new ProductStock(Guid.NewGuid(), 10));

        _productRepository = new Mock<IProductRepository>();
        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_existing);
        _productRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_newCategory);

        _interactor = new ProductUpdateInteractor(
            _productRepository.Object,
            _productCategoryRepository.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private ProductUpdateParam CreateParam(
        string name = "水性ボールペン(黒) 改",
        int price = 150,
        string? imageUrl = "https://example.com/pen.png",
        int quantity = 55)
        => new(_existing.Id, name, price, imageUrl, _newCategory.Id, quantity);

    [TestMethod(DisplayName = "商品情報と在庫数を更新し更新内容を返す")]
    public async Task ExecuteAsync_ValidParam_UpdatesAndReturnsProduct()
    {
        var product = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual("水性ボールペン(黒) 改", product.Name);
        Assert.AreEqual(150, product.Price);
        Assert.AreEqual("https://example.com/pen.png", product.ImageUrl);
        Assert.AreEqual(_newCategory, product.Category);
        Assert.AreEqual(55, product.Stock.Quantity);

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [TestMethod(DisplayName = "商品と在庫の識別IDは更新後も維持される")]
    public async Task ExecuteAsync_ValidParam_PreservesIdentifiers()
    {
        var product = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual(_existing.Id, product.Id);
        Assert.AreEqual(_existing.Stock.Id, product.Stock.Id);
    }

    [TestMethod(DisplayName = "更新操作では論理削除フラグを変更しない")]
    public async Task ExecuteAsync_ValidParam_DoesNotChangeIsDeleted()
    {
        Product? saved = null;
        _productRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => saved = p)
            .Returns(Task.CompletedTask);

        await _interactor.ExecuteAsync(CreateParam());

        Assert.IsNotNull(saved);
        Assert.IsFalse(saved!.IsDeleted);
    }

    [TestMethod(DisplayName = "商品が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_ProductNotFound_ThrowsNotFoundException()
    {
        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "論理削除済みの商品はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_DeletedProduct_ThrowsNotFoundException()
    {
        var deleted = new Product(
            _existing.Id,
            _existing.Name,
            _existing.Price,
            _existing.ImageUrl,
            _oldCategory,
            _existing.Stock,
            isDeleted: true);

        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(deleted);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "商品カテゴリが存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_CategoryNotFound_ThrowsNotFoundException()
    {
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "価格が負数ならDomainExceptionをスローし更新しない")]
    public async Task ExecuteAsync_NegativePrice_ThrowsDomainExceptionAndDoesNotUpdate()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(price: -1)));

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "在庫数が負数ならDomainExceptionをスローし更新しない")]
    public async Task ExecuteAsync_NegativeQuantity_ThrowsDomainExceptionAndDoesNotUpdate()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(quantity: -1)));

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}