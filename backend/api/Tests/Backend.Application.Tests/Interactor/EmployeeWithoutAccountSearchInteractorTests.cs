using Backend.Application.Interactor;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class EmployeeWithoutAccountSearchInteractorTests
{
    private Mock<IEmployeeRepository> _employeeRepository = null!;
    private EmployeeWithoutAccountSearchInteractor _interactor = null!;

    /// <summary>
    /// テスト用の社員を生成する
    /// </summary>
    private static Employee CreateEmployee(string name, string departmentName)
        => new(
            Guid.NewGuid(),
            name,
            null,
            new Department(Guid.NewGuid(), departmentName));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _employeeRepository = new Mock<IEmployeeRepository>();
        _interactor = new EmployeeWithoutAccountSearchInteractor(_employeeRepository.Object);
    }

    [TestMethod(DisplayName = "アカウント未登録の社員をすべて取得する")]
    public async Task ExecuteAsync_ReturnsEmployeesWithoutAccount()
    {
        IReadOnlyList<Employee> expected =
        [
            CreateEmployee("鈴木花子", "商品企画部"),
            CreateEmployee("山本次郎", "販売管理部")
        ];
        _employeeRepository.Setup(r => r.FindWithoutAccountAsync()).ReturnsAsync(expected);

        var employees = await _interactor.ExecuteAsync();

        Assert.HasCount(2, employees);
        Assert.AreEqual("鈴木花子", employees[0].Name);
        Assert.AreEqual("商品企画部", employees[0].Department.Name);
    }

    [TestMethod(DisplayName = "リポジトリの取得処理を1回だけ呼び出す")]
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _employeeRepository.Setup(r => r.FindWithoutAccountAsync()).ReturnsAsync([]);

        await _interactor.ExecuteAsync();

        _employeeRepository.Verify(r => r.FindWithoutAccountAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "登録可能な社員が0件でも例外にせず空の一覧を返す")]
    public async Task ExecuteAsync_NoEmployees_ReturnsEmptyList()
    {
        _employeeRepository.Setup(r => r.FindWithoutAccountAsync()).ReturnsAsync([]);

        var employees = await _interactor.ExecuteAsync();

        Assert.HasCount(0, employees);
    }
}