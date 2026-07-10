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
public class ProductRegisterInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private ProductRegisterInteractor _interactor = null!;

    private ProductCategory _category = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _category = new ProductCategory(Guid.NewGuid(), "文房具");

        _productRepository = new Mock<IProductRepository>();
        _productRepository
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_category);

        _interactor = new ProductRegisterInteractor(
            _productRepository.Object,
            _productCategoryRepository.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private ProductRegisterParam CreateParam(
        string name = "水性ボールペン(黒)",
        int price = 120,
        string? imageUrl = null,
        int quantity = 10)
        => new(name, price, imageUrl, _category.Id, quantity);

    [TestMethod(DisplayName = "商品を登録し登録内容を返す")]
    public async Task ExecuteAsync_ValidParam_RegistersAndReturnsProduct()
    {
        var product = await _interactor.ExecuteAsync(CreateParam(imageUrl: "https://example.com/pen.png"));

        Assert.AreEqual("水性ボールペン(黒)", product.Name);
        Assert.AreEqual(120, product.Price);
        Assert.AreEqual("https://example.com/pen.png", product.ImageUrl);
        Assert.AreEqual(_category, product.Category);
        Assert.AreEqual(10, product.Stock.Quantity);
        Assert.IsFalse(product.IsDeleted);

        _productRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [TestMethod(DisplayName = "商品と在庫の識別IDはユースケースで採番される")]
    public async Task ExecuteAsync_ValidParam_GeneratesProductAndStockIds()
    {
        var product = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreNotEqual(Guid.Empty, product.Id);
        Assert.AreNotEqual(Guid.Empty, product.Stock.Id);
        Assert.AreNotEqual(product.Id, product.Stock.Id);
    }

    [TestMethod(DisplayName = "商品カテゴリが存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_CategoryNotFound_ThrowsNotFoundException()
    {
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "商品カテゴリが存在しない場合は登録を行わない")]
    public async Task ExecuteAsync_CategoryNotFound_DoesNotRegister()
    {
        _productCategoryRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ProductCategory?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _productRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod(DisplayName = "商品名が未指定ならDomainExceptionをスローする")]
    public async Task ExecuteAsync_EmptyName_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(name: "")));
    }

    [TestMethod(DisplayName = "価格が負数ならDomainExceptionをスローする")]
    public async Task ExecuteAsync_NegativePrice_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(price: -1)));
    }

    [TestMethod(DisplayName = "在庫数が負数ならDomainExceptionをスローする")]
    public async Task ExecuteAsync_NegativeQuantity_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(quantity: -1)));
    }

    [TestMethod(DisplayName = "ドメインの不変条件に反する場合は登録を行わない")]
    public async Task ExecuteAsync_InvalidDomainValue_DoesNotRegister()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(
            () => _interactor.ExecuteAsync(CreateParam(price: -1)));

        _productRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }
}