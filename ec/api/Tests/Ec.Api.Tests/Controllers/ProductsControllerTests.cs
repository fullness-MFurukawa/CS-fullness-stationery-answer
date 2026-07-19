using Ec.Api.Adapters;
using Ec.Api.Controllers;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace Ec.Api.Tests.Controllers;

[TestClass]
[TestCategory("Ec.Api.Controllers")]
public class ProductsControllerTests
{
    private Mock<IProductSearchUsecase> _searchUsecase = null!;
    private Mock<IProductDetailUsecase> _detailUsecase = null!;
    private ProductsController _controller = null!;

    private static Product CreateProduct(string name)
        => new(
            Guid.NewGuid(), name, 120, null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 10));

    [TestInitialize]
    public void SetUp()
    {
        _searchUsecase = new Mock<IProductSearchUsecase>();
        _detailUsecase = new Mock<IProductDetailUsecase>();
        _controller = new ProductsController(
            _searchUsecase.Object,
            _detailUsecase.Object,
            new ProductResponseAdapter())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod(DisplayName = "商品検索は200と商品一覧を返す")]
    public async Task SearchAsync_ReturnsOkWithProducts()
    {
        _searchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<ProductSearchParam>()))
            .ReturnsAsync([CreateProduct("ボールペン"), CreateProduct("ノート")]);

        var result = await _controller.SearchAsync(null);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var response = ok!.Value as IReadOnlyList<ProductResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(2, response);
    }

    [TestMethod(DisplayName = "カテゴリIDを指定するとユースケースへ渡される")]
    public async Task SearchAsync_WithCategoryId_PassesToUsecase()
    {
        ProductSearchParam? captured = null;
        var categoryId = Guid.NewGuid();
        _searchUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<ProductSearchParam>()))
            .Callback<ProductSearchParam>(p => captured = p)
            .ReturnsAsync([]);

        await _controller.SearchAsync(categoryId);

        Assert.IsNotNull(captured);
        Assert.AreEqual(categoryId, captured!.CategoryId);
    }

    [TestMethod(DisplayName = "商品詳細は200と商品を返す")]
    public async Task GetAsync_ReturnsOkWithProduct()
    {
        var product = CreateProduct("ボールペン");
        _detailUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(product);

        var result = await _controller.GetAsync(product.Id);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var response = ok!.Value as ProductResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("ボールペン", response!.Name);
    }
}