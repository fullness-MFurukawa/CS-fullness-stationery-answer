using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Factories;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class EmployeeRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private EmployeeRepository CreateRepository()
        => new(Context, new EmployeeFactory(new DepartmentAdapter(), new EmployeeAdapter()));

    [TestMethod(DisplayName = "アカウント未登録の社員を社員ID順に取得できる")]
    public async Task FindWithoutAccountAsync_ReturnsEmployeesWithoutAccountOrderedById()
    {
        var repository = CreateRepository();

        var employees = await repository.FindWithoutAccountAsync();

        Assert.HasCount(3, employees);
        Assert.AreEqual("鈴木花子", employees[0].Name);
        Assert.AreEqual("山本次郎", employees[1].Name);
        Assert.AreEqual("佐藤三郎", employees[2].Name);
    }

    [TestMethod(DisplayName = "アカウント登録済みの社員は取得されない")]
    public async Task FindWithoutAccountAsync_ExcludesEmployeeWithAccount()
    {
        var repository = CreateRepository();

        var employees = await repository.FindWithoutAccountAsync();

        Assert.IsFalse(employees.Any(e => e.Name == "フルネス太郎"));
    }

    [TestMethod(DisplayName = "取得した社員に所属部署が含まれる")]
    public async Task FindWithoutAccountAsync_IncludesDepartment()
    {
        var repository = CreateRepository();

        var employees = await repository.FindWithoutAccountAsync();

        Assert.AreEqual("商品企画部", employees[0].Department.Name);
        Assert.AreEqual("販売管理部", employees[1].Department.Name);
        Assert.AreEqual("販売管理部", employees[2].Department.Name);
    }

    [TestMethod(DisplayName = "識別IDを指定して社員を取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsEmployee()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindWithoutAccountAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual("鈴木花子", actual.Name);
        Assert.AreEqual("スズキハナコ", actual.NameKana);
    }

    [TestMethod(DisplayName = "識別IDで取得した社員に所属部署が含まれる")]
    public async Task FindByIdAsync_ExistingId_IncludesDepartment()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindWithoutAccountAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual("商品企画部", actual.Department.Name);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }
}