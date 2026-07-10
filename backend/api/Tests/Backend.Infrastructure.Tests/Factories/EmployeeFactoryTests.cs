using Backend.Domain.Exceptions;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Factories;

using EfDepartment = Backend.Infrastructure.Entities.Department;
using EfEmployee = Backend.Infrastructure.Entities.Employee;

namespace Backend.Infrastructure.Tests.Factories;

[TestClass]
[TestCategory("Backend.Infrastructure.Factories")]
public class EmployeeFactoryTests
{
    private readonly EmployeeFactory _factory = new(
        new DepartmentAdapter(),
        new EmployeeAdapter());

    /// <summary>
    /// テスト用のEF社員エンティティを生成する
    /// </summary>
    /// <param name="withDepartment">部署をロード済みにするか</param>
    /// <param name="nameKana">社員名カナ</param>
    private static EfEmployee CreateEntity(bool withDepartment = true, string? nameKana = "フルネスタロウ")
    {
        var entity = new EfEmployee
        {
            Id = 1,
            EmployeeUuid = Guid.NewGuid(),
            Name = "フルネス太郎",
            NameKana = nameKana,
            DepartmentId = 2
        };

        if (withDepartment)
        {
            entity.Department = new EfDepartment
            {
                Id = 2,
                DepartmentUuid = Guid.NewGuid(),
                Name = "販売管理部"
            };
        }

        return entity;
    }

    [TestMethod(DisplayName = "部署がロード済みなら社員集約を組み立てられる")]
    public void Create_WithLoadedDepartment_ReturnsEmployeeAggregate()
    {
        var entity = CreateEntity();

        var employee = _factory.Create(entity);

        Assert.AreEqual(entity.EmployeeUuid, employee.Id);
        Assert.AreEqual("フルネス太郎", employee.Name);
        Assert.AreEqual("フルネスタロウ", employee.NameKana);
    }

    [TestMethod(DisplayName = "組み立てた社員集約に部署が含まれる")]
    public void Create_WithLoadedDepartment_IncludesDepartment()
    {
        var entity = CreateEntity();

        var employee = _factory.Create(entity);

        Assert.AreEqual(entity.Department.DepartmentUuid, employee.Department.Id);
        Assert.AreEqual("販売管理部", employee.Department.Name);
    }

    [TestMethod(DisplayName = "社員名カナがnullでも組み立てられる")]
    public void Create_NullNameKana_ReturnsEmployeeAggregate()
    {
        var entity = CreateEntity(nameKana: null);

        var employee = _factory.Create(entity);

        Assert.IsNull(employee.NameKana);
    }

    [TestMethod(DisplayName = "部署が未ロードならDomainExceptionをスローする")]
    public void Create_DepartmentNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withDepartment: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }
}