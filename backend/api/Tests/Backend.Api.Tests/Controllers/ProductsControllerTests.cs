using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class ProductsControllerTests
{
    private Mock<IProductSearchUsecase> _productSearchUsecase = null!;
    private Mock<IProductRegisterUsecase> _productRegisterUsecase = null!;
    private Mock<IProductUpdateUsecase> _productUpdateUsecase = null!;
    private Mock<IProductDeleteUsecase> _productDeleteUsecase = null!;
    private ProductsController _controller = null!;

    private ProductCategory _category = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    /// <remarks>
    /// アダプタは単純な変換処理のため、モック化せず実インスタンスを使用する。
    /// これによりアダプタの変換ロジックもあわせて検証される。
    /// </remarks>
    [TestInitialize]
    public void SetUp()
    {
        _category = new ProductCategory(Guid.NewGuid(), "文房具");

        _productSearchUsecase = new Mock<IProductSearchUsecase>();
        _productRegisterUsecase = new Mock<IProductRegisterUsecase>();
        _productUpdateUsecase = new Mock<IProductUpdateUsecase>();
        _productDeleteUsecase = new Mock<IProductDeleteUsecase>();

        _controller = new ProductsController(
            _productSearchUsecase.Object,
            _productRegisterUsecase.Object,
            _productUpdateUsecase.Object,
            _productDeleteUsecase.Object,
            new ProductRegisterRequestAdapter(),
            new ProductUpdateRequestAdapter(),
            new ProductResponseAdapter());
    }

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private Product CreateProduct(string name = "水性ボールペン(黒)", string? imageUrl = null)
        => new(
            Guid.NewGuid(),
            name,
            120,
            imageUrl,
            _category,
            new ProductStock(Guid.NewGuid(), 10));

    [TestMethod(DisplayName = "商品検索は200と商品一覧を返す")]
    public async Task SearchAsync_ReturnsOkWithProducts()
    {
        var products = new List<Product> { CreateProduct("商品A"), CreateProduct("商品B") };
        _productSearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(products);

        var result = await _controller.SearchAsync(null);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as IReadOnlyList<ProductResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(2, response!);
        Assert.AreEqual("商品A", response![0].Name);
    }

    [TestMethod(DisplayName = "商品検索は該当0件で空配列を返す")]
    public async Task SearchAsync_NoProducts_ReturnsEmptyList()
    {
        _productSearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new List<Product>());

        var result = await _controller.SearchAsync(null);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var response = ok!.Value as IReadOnlyList<ProductResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(0, response!);
    }

    [TestMethod(DisplayName = "商品検索はカテゴリIDをユースケースへ渡す")]
    public async Task SearchAsync_PassesCategoryIdToUsecase()
    {
        var categoryId = Guid.NewGuid();
        _productSearchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid?>()))
            .ReturnsAsync(new List<Product>());

        await _controller.SearchAsync(categoryId);

        _productSearchUsecase.Verify(u => u.ExecuteAsync(categoryId), Times.Once);
    }

    [TestMethod(DisplayName = "商品登録は201と登録内容を返す")]
    public async Task RegisterAsync_ReturnsCreatedWithProduct()
    {
        var product = CreateProduct("新商品", "https://example.com/new.png");
        _productRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Backend.Application.Params.ProductRegisterParam>()))
            .ReturnsAsync(product);

        var request = new ProductRegisterRequest("新商品", 120, _category.Id, 10, null);
        var result = await _controller.RegisterAsync(request);

        var created = result.Result as ObjectResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);

        var response = created.Value as ProductResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("新商品", response!.Name);
    }

    [TestMethod(DisplayName = "商品修正は200と更新内容を返す")]
    public async Task UpdateAsync_ReturnsOkWithProduct()
    {
        var product = CreateProduct("修正後商品");
        _productUpdateUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Backend.Application.Params.ProductUpdateParam>()))
            .ReturnsAsync(product);

        var request = new ProductUpdateRequest("修正後商品", 150, _category.Id, 20, null);
        var result = await _controller.UpdateAsync(Guid.NewGuid(), request);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as ProductResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("修正後商品", response!.Name);
    }

    [TestMethod(DisplayName = "商品修正はルートの商品IDを優先してユースケースへ渡す")]
    public async Task UpdateAsync_UsesRouteProductId()
    {
        var routeProductId = Guid.NewGuid();
        _productUpdateUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Backend.Application.Params.ProductUpdateParam>()))
            .ReturnsAsync(CreateProduct());

        // ボディには別のProductIdを入れ、ルートの値が優先されることを確認する
        var request = new ProductUpdateRequest("商品", 150, _category.Id, 20, null)
        {
            ProductId = Guid.NewGuid(),
        };
        await _controller.UpdateAsync(routeProductId, request);

        _productUpdateUsecase.Verify(
            u => u.ExecuteAsync(It.Is<Backend.Application.Params.ProductUpdateParam>(
                p => p.ProductId == routeProductId)),
            Times.Once);
    }

    [TestMethod(DisplayName = "商品削除は200と削除された商品を返す")]
    public async Task DeleteAsync_ReturnsOkWithProduct()
    {
        var product = CreateProduct("削除対象");
        _productDeleteUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(product);

        var productId = Guid.NewGuid();
        var result = await _controller.DeleteAsync(productId);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as ProductResponse;
        Assert.IsNotNull(response);
    }

    [TestMethod(DisplayName = "商品削除はルートの商品IDをユースケースへ渡す")]
    public async Task DeleteAsync_PassesProductIdToUsecase()
    {
        var productId = Guid.NewGuid();
        _productDeleteUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateProduct());

        await _controller.DeleteAsync(productId);

        _productDeleteUsecase.Verify(u => u.ExecuteAsync(productId), Times.Once);
    }
}