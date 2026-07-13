using Backend.Api.Adapters;
using Backend.Api.Controllers;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;
using Backend.Domain.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

namespace Backend.Api.Tests.Controllers;

[TestClass]
[TestCategory("Backend.Api.Controllers")]
public class EmployeesControllerTests
{
    private Mock<IEmployeeWithoutAccountSearchUsecase> _employeeWithoutAccountSearchUsecase = null!;
    private EmployeesController _controller = null!;

    private Department _department = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _department = new Department(Guid.NewGuid(), "商品企画部");

        _employeeWithoutAccountSearchUsecase = new Mock<IEmployeeWithoutAccountSearchUsecase>();

        _controller = new EmployeesController(
            _employeeWithoutAccountSearchUsecase.Object,
            new EmployeeResponseAdapter());
    }

    /// <summary>
    /// テスト用の社員を生成する
    /// </summary>
    private Employee CreateEmployee(string name = "鈴木花子", string? nameKana = "スズキハナコ")
        => new(Guid.NewGuid(), name, nameKana, _department);

    [TestMethod(DisplayName = "アカウント未登録の社員一覧は200と社員一覧を返す")]
    public async Task SearchWithoutAccountAsync_ReturnsOkWithEmployees()
    {
        var employees = new List<Employee> { CreateEmployee("鈴木花子"), CreateEmployee("山本次郎") };
        _employeeWithoutAccountSearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(employees);

        var result = await _controller.SearchWithoutAccountAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var response = ok.Value as IReadOnlyList<EmployeeResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(2, response!);
        Assert.AreEqual("鈴木花子", response![0].Name);
        Assert.AreEqual("商品企画部", response[0].DepartmentName);
    }

    [TestMethod(DisplayName = "アカウント未登録の社員一覧は該当0件で空配列を返す")]
    public async Task SearchWithoutAccountAsync_NoEmployees_ReturnsEmptyList()
    {
        _employeeWithoutAccountSearchUsecase
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(new List<Employee>());

        var result = await _controller.SearchWithoutAccountAsync();

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var response = ok!.Value as IReadOnlyList<EmployeeResponse>;
        Assert.IsNotNull(response);
        Assert.HasCount(0, response!);
    }
}