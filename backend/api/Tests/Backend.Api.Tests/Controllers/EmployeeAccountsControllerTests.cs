using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class EmployeeAccountsControllerTests
{
    private Mock<IEmployeeAccountRegisterUsecase> _employeeAccountRegisterUsecase = null!;
    private EmployeeAccountsController _controller = null!;

    private Employee _employee = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        var department = new Department(Guid.NewGuid(), "商品企画部");
        _employee = new Employee(Guid.NewGuid(), "鈴木花子", "スズキハナコ", department);

        _employeeAccountRegisterUsecase = new Mock<IEmployeeAccountRegisterUsecase>();

        _controller = new EmployeeAccountsController(
            _employeeAccountRegisterUsecase.Object,
            new EmployeeAccountRegisterRequestAdapter(),
            new EmployeeAccountResponseAdapter());
    }

    [TestMethod(DisplayName = "担当者アカウント登録は201と登録内容を返す")]
    public async Task RegisterAsync_ReturnsCreatedWithAccount()
    {
        var account = new EmployeeAccount(Guid.NewGuid(), "hanako01", "hashed-password", _employee);
        _employeeAccountRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<EmployeeAccountRegisterParam>()))
            .ReturnsAsync(account);

        var request = new EmployeeAccountRegisterRequest(_employee.Id, "hanako01", "Password123");
        var result = await _controller.RegisterAsync(request);

        var created = result.Result as ObjectResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);

        var response = created.Value as EmployeeAccountResponse;
        Assert.IsNotNull(response);
        Assert.AreEqual("hanako01", response!.AccountName);
        Assert.AreEqual("鈴木花子", response.EmployeeName);
    }

    [TestMethod(DisplayName = "担当者アカウント登録は入力値をユースケースへ渡す")]
    public async Task RegisterAsync_PassesParamToUsecase()
    {
        var account = new EmployeeAccount(Guid.NewGuid(), "hanako01", "hashed-password", _employee);
        _employeeAccountRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<EmployeeAccountRegisterParam>()))
            .ReturnsAsync(account);

        var request = new EmployeeAccountRegisterRequest(_employee.Id, "hanako01", "Password123");
        await _controller.RegisterAsync(request);

        _employeeAccountRegisterUsecase.Verify(
            u => u.ExecuteAsync(It.Is<EmployeeAccountRegisterParam>(
                p => p.EmployeeId == _employee.Id
                  && p.AccountName == "hanako01"
                  && p.Password == "Password123")),
            Times.Once);
    }

    [TestMethod(DisplayName = "レスポンスにパスワードに相当するプロパティが存在しない")]
    public async Task RegisterAsync_ResponseHasNoPasswordProperty()
    {
        var account = new EmployeeAccount(Guid.NewGuid(), "hanako01", "hashed-password", _employee);
        _employeeAccountRegisterUsecase
            .Setup(u => u.ExecuteAsync(It.IsAny<EmployeeAccountRegisterParam>()))
            .ReturnsAsync(account);

        var request = new EmployeeAccountRegisterRequest(_employee.Id, "hanako01", "Password123");
        var result = await _controller.RegisterAsync(request);

        var response = (result.Result as ObjectResult)!.Value as EmployeeAccountResponse;

        // EmployeeAccountResponse の公開プロパティに Password を含まないことを型レベルで確認する
        var hasPasswordProperty = typeof(EmployeeAccountResponse)
            .GetProperties()
            .Any(p => p.Name.Contains("Password", StringComparison.OrdinalIgnoreCase));

        Assert.IsFalse(hasPasswordProperty);
        Assert.IsNotNull(response);
    }
}