using Backend.Domain.Exceptions;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Factories;

using EfDepartment = Backend.Infrastructure.Entities.Department;
using EfEmployee = Backend.Infrastructure.Entities.Employee;
using EfEmployeeAccount = Backend.Infrastructure.Entities.EmployeeAccount;

namespace Backend.Infrastructure.Tests.Factories;

[TestClass]
[TestCategory("Backend.Infrastructure.Factories")]
public class EmployeeAccountFactoryTests
{
    private readonly EmployeeAccountFactory _factory = new(
        new EmployeeFactory(new DepartmentAdapter(), new EmployeeAdapter()),
        new EmployeeAccountAdapter());

    /// <summary>
    /// テスト用のEF社員アカウントエンティティを生成する
    /// </summary>
    /// <param name="withEmployee">社員をロード済みにするか</param>
    /// <param name="withDepartment">部署をロード済みにするか</param>
    private static EfEmployeeAccount CreateEntity(bool withEmployee = true, bool withDepartment = true)
    {
        var entity = new EfEmployeeAccount
        {
            Id = 1,
            AccountUuid = Guid.NewGuid(),
            Name = "fullness",
            Password = "hashed-password",
            EmployeeId = 1
        };

        if (withEmployee)
        {
            var employee = new EfEmployee
            {
                Id = 1,
                EmployeeUuid = Guid.NewGuid(),
                Name = "フルネス太郎",
                NameKana = "フルネスタロウ",
                DepartmentId = 2
            };

            if (withDepartment)
            {
                employee.Department = new EfDepartment
                {
                    Id = 2,
                    DepartmentUuid = Guid.NewGuid(),
                    Name = "販売管理部"
                };
            }

            entity.Employee = employee;
        }

        return entity;
    }

    [TestMethod(DisplayName = "関連がロード済みならアカウント集約を組み立てられる")]
    public void Create_WithLoadedRelations_ReturnsAccountAggregate()
    {
        var entity = CreateEntity();

        var account = _factory.Create(entity);

        Assert.AreEqual(entity.AccountUuid, account.Id);
        Assert.AreEqual("fullness", account.Name);
        Assert.AreEqual("hashed-password", account.Password);
    }

    [TestMethod(DisplayName = "組み立てたアカウント集約に社員と部署が含まれる")]
    public void Create_WithLoadedRelations_IncludesEmployeeAndDepartment()
    {
        var entity = CreateEntity();

        var account = _factory.Create(entity);

        Assert.AreEqual(entity.Employee.EmployeeUuid, account.Employee.Id);
        Assert.AreEqual("フルネス太郎", account.Employee.Name);
        Assert.AreEqual(entity.Employee.Department.DepartmentUuid, account.Employee.Department.Id);
        Assert.AreEqual("販売管理部", account.Employee.Department.Name);
    }

    [TestMethod(DisplayName = "社員が未ロードならDomainExceptionをスローする")]
    public void Create_EmployeeNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withEmployee: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "社員はロード済みでも部署が未ロードならDomainExceptionをスローする")]
    public void Create_DepartmentNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withDepartment: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }
}