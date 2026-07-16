using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Tests.Fakes;
using Backend.Application.Usecases;
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
    private Mock<IImageUploadUsecase> _imageUploadUsecase = null!;
    private Mock<IImageStorage> _imageStorage = null!;
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
            "https://example.com/old.png",
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

        _imageUploadUsecase = new Mock<IImageUploadUsecase>();
        _imageUploadUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<ImageUploadParam>()))
            .ReturnsAsync("https://example.com/new.png");

        _imageStorage = new Mock<IImageStorage>();
        _imageStorage
            .Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _interactor = new ProductUpdateInteractor(
            _productRepository.Object,
            _productCategoryRepository.Object,
            _imageUploadUsecase.Object,
            _imageStorage.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する（画像なし＝既存維持）
    /// </summary>
    private ProductUpdateParam CreateParam(
        string name = "水性ボールペン(黒) 改",
        int price = 150,
        int quantity = 55)
        => new(_existing.Id, name, price, _newCategory.Id, quantity);

    /// <summary>
    /// テスト用の入力値を生成する（画像あり＝差し替え）
    /// </summary>
    private ProductUpdateParam CreateParamWithImage(
        string name = "水性ボールペン(黒) 改",
        int price = 150,
        int quantity = 55)
        => new(
            _existing.Id,
            name,
            price,
            _newCategory.Id,
            quantity,
            new MemoryStream([0x89, 0x50, 0x4E, 0x47]),
            "pen.png",
            "image/png",
            4);

    [TestMethod(DisplayName = "商品情報と在庫数を更新し更新内容を返す")]
    public async Task ExecuteAsync_ValidParam_UpdatesAndReturnsProduct()
    {
        var product = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual("水性ボールペン(黒) 改", product.Name);
        Assert.AreEqual(150, product.Price);
        Assert.AreEqual(_newCategory, product.Category);
        Assert.AreEqual(55, product.Stock.Quantity);

        _productRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [TestMethod(DisplayName = "画像を指定しない場合は既存の画像URLを維持する")]
    public async Task ExecuteAsync_NoImage_KeepsExistingImageUrl()
    {
        var product = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual("https://example.com/old.png", product.ImageUrl);
        _imageUploadUsecase.Verify(u => u.ExecuteAsync(It.IsAny<ImageUploadParam>()), Times.Never);
        _imageStorage.Verify(s => s.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [TestMethod(DisplayName = "画像を指定した場合は新しい画像URLへ差し替える")]
    public async Task ExecuteAsync_WithImage_ReplacesImageUrl()
    {
        var product = await _interactor.ExecuteAsync(CreateParamWithImage());

        Assert.AreEqual("https://example.com/new.png", product.ImageUrl);
    }

    [TestMethod(DisplayName = "画像を差し替えた場合は更新後に古い画像を削除する")]
    public async Task ExecuteAsync_WithImage_DeletesOldImage()
    {
        await _interactor.ExecuteAsync(CreateParamWithImage());

        _imageStorage.Verify(s => s.DeleteAsync("https://example.com/old.png"), Times.Once);
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


    [TestMethod(DisplayName = "画像の削除を指定した場合は画像URLをnullにする")]
    public async Task ExecuteAsync_RemoveImage_SetsImageUrlToNull()
    {
        var param = CreateParam() with { RemoveImage = true };

        var product = await _interactor.ExecuteAsync(param);

        Assert.IsNull(product.ImageUrl);
    }

    [TestMethod(DisplayName = "画像の削除を指定した場合は更新後に既存の画像を削除する")]
    public async Task ExecuteAsync_RemoveImage_DeletesExistingImage()
    {
        var param = CreateParam() with { RemoveImage = true };

        await _interactor.ExecuteAsync(param);

        _imageStorage.Verify(s => s.DeleteAsync("https://example.com/old.png"), Times.Once);
    }

    [TestMethod(DisplayName = "画像の削除を指定した場合は画像アップロードを行わない")]
    public async Task ExecuteAsync_RemoveImage_DoesNotUploadImage()
    {
        var param = CreateParam() with { RemoveImage = true };

        await _interactor.ExecuteAsync(param);

        _imageUploadUsecase.Verify(u => u.ExecuteAsync(It.IsAny<ImageUploadParam>()), Times.Never);
    }
}