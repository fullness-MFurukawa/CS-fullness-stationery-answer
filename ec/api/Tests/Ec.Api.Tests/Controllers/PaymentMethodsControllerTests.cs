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
public class PaymentMethodsControllerTests
{
    private Mock<IPaymentMethodSearchUsecase> _usecase = null!;
    private PaymentMethodsController _controller = null!;

    [TestInitialize]
    public void SetUp()
    {
        _usecase = new Mock<IPaymentMethodSearchUsecase>();
        _controller = new PaymentMethodsController(_usecase.Object, new PaymentMethodResponseAdapter())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod(DisplayName = "支払い方法一覧は200と一覧を返す")]
    public async Task SearchAsync_ReturnsOkWithPaymentMethods()
    {
        _usecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync([new PaymentMethod(1, "現金")]);

        var result = await _controller.SearchAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var response = ok!.Value as IEnumerable<PaymentMethodResponse>;
        Assert.IsNotNull(response);
        Assert.AreEqual(1, response!.Count());
    }
}