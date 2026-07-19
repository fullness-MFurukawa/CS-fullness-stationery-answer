using Ec.Api.Adapters;
using Ec.Api.Controllers;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
namespace Ec.Api.Tests.Controllers;

[TestClass]
[TestCategory("Ec.Api.Controllers")]
public class CategoriesControllerTests
{
    private Mock<ICategorySearchUsecase> _usecase = null!;
    private CategoriesController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _usecase = new Mock<ICategorySearchUsecase>();
        _controller = new CategoriesController(_usecase.Object, new CategoryResponseAdapter())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod(DisplayName = "カテゴリ一覧は200とカテゴリを返す")]
    public async Task SearchAsync_ReturnsOkWithCategories()
    {
        _usecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync([
                new ProductCategory(Guid.NewGuid(), "文房具"),
                new ProductCategory(Guid.NewGuid(), "雑貨"),
            ]);

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var response = ok!.Value as IEnumerable<CategoryResponse>;
        Assert.IsNotNull(response);
        Assert.AreEqual(2, response!.Count());
    }
}