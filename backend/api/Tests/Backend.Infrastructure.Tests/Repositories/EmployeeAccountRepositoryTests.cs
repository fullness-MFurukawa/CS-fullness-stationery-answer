using Backend.Domain.Models;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Exceptions;
using Backend.Infrastructure.Factories;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class EmployeeAccountRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private EmployeeAccountRepository CreateRepository()
    {
        var employeeFactory = new EmployeeFactory(new DepartmentAdapter(), new EmployeeAdapter());
        var accountAdapter = new EmployeeAccountAdapter();

        return new EmployeeAccountRepository(
            Context,
            accountAdapter,
            new EmployeeAccountFactory(employeeFactory, accountAdapter));
    }

    /// <summary>
    /// アカウント未登録の社員を1名取得する
    /// </summary>
    private async Task<Employee> GetEmployeeWithoutAccountAsync()
    {
        var employeeRepository = new EmployeeRepository(
            Context,
            new EmployeeFactory(new DepartmentAdapter(), new EmployeeAdapter()));

        return (await employeeRepository.FindWithoutAccountAsync())[0];
    }

    [TestMethod(DisplayName = "アカウント名を指定して社員アカウントを取得できる")]
    public async Task FindByAccountNameAsync_ExistingName_ReturnsAccount()
    {
        var repository = CreateRepository();

        var account = await repository.FindByAccountNameAsync("fullness");

        Assert.IsNotNull(account);
        Assert.AreEqual("fullness", account.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(account.Password));
    }

    [TestMethod(DisplayName = "取得したアカウントに社員と部署が含まれる")]
    public async Task FindByAccountNameAsync_ExistingName_IncludesEmployeeAndDepartment()
    {
        var repository = CreateRepository();

        var account = await repository.FindByAccountNameAsync("fullness");

        Assert.IsNotNull(account);
        Assert.AreEqual("フルネス太郎", account.Employee.Name);
        Assert.AreEqual("販売管理部", account.Employee.Department.Name);
    }

    [TestMethod(DisplayName = "存在しないアカウント名を指定するとnullを返す")]
    public async Task FindByAccountNameAsync_NotExistingName_ReturnsNull()
    {
        var repository = CreateRepository();

        var account = await repository.FindByAccountNameAsync("notexist");

        Assert.IsNull(account);
    }

    [TestMethod(DisplayName = "登録済みのアカウント名は存在すると判定される")]
    public async Task ExistsByAccountNameAsync_ExistingName_ReturnsTrue()
    {
        var repository = CreateRepository();

        var exists = await repository.ExistsByAccountNameAsync("fullness");

        Assert.IsTrue(exists);
    }

    [TestMethod(DisplayName = "未登録のアカウント名は存在しないと判定される")]
    public async Task ExistsByAccountNameAsync_NotExistingName_ReturnsFalse()
    {
        var repository = CreateRepository();

        var exists = await repository.ExistsByAccountNameAsync("notexist");

        Assert.IsFalse(exists);
    }

    [TestMethod(DisplayName = "社員アカウントを新規登録するとアカウント名で取得できる")]
    public async Task AddAsync_NewAccount_CanBeFoundByAccountName()
    {
        var repository = CreateRepository();
        var employee = await GetEmployeeWithoutAccountAsync();
        var newAccount = new EmployeeAccount(Guid.NewGuid(), "hanako01", "hashed-password", employee);

        await repository.AddAsync(newAccount);

        var saved = await repository.FindByAccountNameAsync("hanako01");
        Assert.IsNotNull(saved);
        Assert.AreEqual(newAccount.Id, saved.Id);
        Assert.AreEqual("hashed-password", saved.Password);
    }

    [TestMethod(DisplayName = "新規登録したアカウントに指定した社員が紐づく")]
    public async Task AddAsync_NewAccount_ResolvesEmployeeForeignKey()
    {
        var repository = CreateRepository();
        var employee = await GetEmployeeWithoutAccountAsync();
        var newAccount = new EmployeeAccount(Guid.NewGuid(), "hanako01", "hashed-password", employee);

        await repository.AddAsync(newAccount);

        var saved = await repository.FindByAccountNameAsync("hanako01");
        Assert.IsNotNull(saved);
        Assert.AreEqual(employee.Id, saved.Employee.Id);
        Assert.AreEqual("鈴木花子", saved.Employee.Name);
    }

    [TestMethod(DisplayName = "重複したアカウント名で登録するとInternalExceptionをスローする")]
    public async Task AddAsync_DuplicatedAccountName_ThrowsInternalException()
    {
        var repository = CreateRepository();
        var employee = await GetEmployeeWithoutAccountAsync();
        var duplicated = new EmployeeAccount(Guid.NewGuid(), "fullness", "hashed-password", employee);

        await Assert.ThrowsExactlyAsync<InternalException>(() => repository.AddAsync(duplicated));
    }
}