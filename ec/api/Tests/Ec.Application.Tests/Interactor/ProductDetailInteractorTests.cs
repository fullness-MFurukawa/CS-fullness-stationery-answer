using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class ProductDetailInteractorTests
{
    private Mock<IProductRepository> _productRepository = null!;
    private ProductDetailInteractor _interactor = null!;

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(Guid id, string name = "水性ボールペン(黒)")
        => new(
            id,
            name,
            150,
            null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _productRepository = new Mock<IProductRepository>();
        _interactor = new ProductDetailInteractor(_productRepository.Object);
    }

    [TestMethod(DisplayName = "存在する商品を返す")]
    public async Task ExecuteAsync_ExistingProduct_ReturnsProduct()
    {
        var id = Guid.NewGuid();
        var product = CreateProduct(id);
        _productRepository
            .Setup(r => r.FindByIdAsync(id))
            .ReturnsAsync(product);

        var result = await _interactor.ExecuteAsync(id);

        Assert.AreEqual(id, result.Id);
        Assert.AreEqual("水性ボールペン(黒)", result.Name);
    }

    [TestMethod(DisplayName = "商品が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_NotFound_ThrowsNotFoundException()
    {
        // リポジトリは論理削除された商品もnullを返すため、
        // 「存在しない」と「削除済み」はここでは区別されない
        _productRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(Guid.NewGuid()));
    }
}