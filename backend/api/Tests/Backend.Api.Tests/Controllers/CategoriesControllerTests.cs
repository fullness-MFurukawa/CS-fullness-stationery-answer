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
public class CategoriesControllerTests
{
    private Mock<ICategorySearchUsecase> _categorySearchUsecase = null!;
    private Mock<ICategoryRegisterUsecase> _categoryRegisterUsecase = null!;
    private CategoriesController _controller = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _categorySearchUsecase = new Mock<ICategorySearchUsecase>();
        _categoryRegisterUsecase = new Mock<ICategoryRegisterUsecase>();

        _controller = new CategoriesController(
            _categorySearchUsecase.Object,
            _categoryRegisterUsecase.Object,
            new CategoryResponseAdapter());
    }

    [TestMethod(DisplayName = "商品カテゴリ一覧は200とカテゴリ一覧を返す")]
    public async Task SearchAsync_ReturnsOkWithCategories()
    {
        var categories = new List<ProductCategory>
        {
            new(Guid.NewGuid(), "文房具"),
            new(Guid.NewGuid(), "雑貨"),
        };
        _categorySearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(categories);

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as IEnumerable<CategoryResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(2, response!.ToList());
    }

    [TestMethod(DisplayName = "商品カテゴリ一覧は該当0件で空配列を返す")]
    public async Task SearchAsync_NoCategories_ReturnsEmptyList()
    {
        _categorySearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(new List<ProductCategory>());

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var response = ok!.Value as IEnumerable<CategoryResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(0, response!.ToList());
    }

    [TestMethod(DisplayName = "商品カテゴリ登録は201と登録内容を返す")]
    public async Task RegisterAsync_ReturnsCreatedWithCategory()
    {
        var category = new ProductCategory(Guid.NewGuid(), "画材");
        _categoryRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<string>()))
            .ReturnsAsync(category);

        var request = new CategoryRegisterRequest("画材");
        var result = await _controller.RegisterAsync(request);

        var created = result.Result as ObjectResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);

        var response = created.Value as CategoryResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("画材", response!.Name);
    }

    [TestMethod(DisplayName = "商品カテゴリ登録はカテゴリ名をユースケースへ渡す")]
    public async Task RegisterAsync_PassesNameToUsecase()
    {
        var category = new ProductCategory(Guid.NewGuid(), "画材");
        _categoryRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<string>()))
            .ReturnsAsync(category);

        await _controller.RegisterAsync(new CategoryRegisterRequest("画材"));

        _categoryRegisterUsecase.Verify(u => u.ExecuteAsync("画材"), Times.Once);
    }
}