using EfEmployee = Backend.Infrastructure.Entities.Employee;
using DomainEmployee = Backend.Domain.Models.Employee;
using DomainDepartment = Backend.Domain.Models.Department;
namespace Backend.Infrastructure.Adapters;
/// <summary>
/// 社員(Employee)とEFエンティティ(EmployeeEntity)を相互変換するアダプタ
/// 関連（部署）の変換は行わず、変換済みのものを受け取る
/// </summary>
public class EmployeeAdapter
{
    /// <summary>
    /// EFエンティティと変換済みの部署からドメインの社員を生成
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <param name="department">変換済みの部署</param>
    /// <returns>ドメインの社員</returns>
    public DomainEmployee ToDomain(EfEmployee source, DomainDepartment department)
        => new(source.EmployeeUuid, source.Name, source.NameKana, department);

    /// <summary>
    /// ドメインの社員からEFエンティティへ変換（スカラー項目のみ）
    /// </summary>
    /// <param name="domain">変換元のドメインの社員</param>
    /// <returns>EFエンティティ</returns>
    public EfEmployee ToSource(DomainEmployee domain)
        => new()
        {
            EmployeeUuid = domain.Id,
            Name = domain.Name,
            NameKana = domain.NameKana
        };
}