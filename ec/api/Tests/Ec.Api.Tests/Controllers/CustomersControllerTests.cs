using Ec.Api.Adapters;
using Ec.Api.Controllers;
using Ec.Api.ViewModels.Requests;
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
public class CustomersControllerTests
{
    private Mock<ICustomerRegisterUsecase> _usecase = null!;
    private CustomersController _controller = null!;

    private static CustomerRegisterRequest CreateRequest()
        => new(
            "鈴木花子", "スズキハナコ", "東京都渋谷区", null,
            "090-1111-2222", "hanako@example.com", "hanako01", "pass12345");

    [TestInitialize]
    public void SetUp()
    {
        _usecase = new Mock<ICustomerRegisterUsecase>();
        _controller = new CustomersController(
            _usecase.Object,
            new CustomerRegisterRequestAdapter(),
            new CustomerResponseAdapter())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    [TestMethod(DisplayName = "登録成功は201と顧客情報を返す")]
    public async Task RegisterAsync_Success_ReturnsCreated()
    {
        var customer = new Customer(
            Guid.NewGuid(), "鈴木花子", "スズキハナコ", "東京都渋谷区", null,
            "090-1111-2222", "hanako@example.com", "hanako01", "hashed", DateTime.Now);
        _usecase
            .Setup(u => u.ExecuteAsync(It.IsAny<CustomerRegisterParam>()))
            .ReturnsAsync(customer);

        var result = await _controller.RegisterAsync(CreateRequest());

        var created = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);
        var response = created.Value as CustomerResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("hanako@example.com", response!.MailAddress);
    }

    [TestMethod(DisplayName = "リクエストの入力値がユースケースへ渡される")]
    public async Task RegisterAsync_PassesParamToUsecase()
    {
        CustomerRegisterParam? captured = null;
        var customer = new Customer(
            Guid.NewGuid(), "鈴木花子", "スズキハナコ", "東京都渋谷区", null,
            "090-1111-2222", "hanako@example.com", "hanako01", "hashed", DateTime.Now);
        _usecase
            .Setup(u => u.ExecuteAsync(It.IsAny<CustomerRegisterParam>()))
            .Callback<CustomerRegisterParam>(p => captured = p)
            .ReturnsAsync(customer);

        await _controller.RegisterAsync(CreateRequest());

        Assert.IsNotNull(captured);
        Assert.AreEqual("hanako@example.com", captured!.MailAddress);
        Assert.AreEqual("hanako01", captured.Username);
    }
}