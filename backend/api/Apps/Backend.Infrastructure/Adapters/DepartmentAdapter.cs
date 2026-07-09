using Backend.Domain.Adapters;
using EfDepartment = Backend.Infrastructure.Entities.Department;
using DomainDepartment = Backend.Domain.Models.Department;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 部署のEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class DepartmentAdapter : IEntityAdapter<EfDepartment, DomainDepartment>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainDepartment ToDomain(EfDepartment source)
        => new(source.DepartmentUuid, source.Name);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfDepartment ToSource(DomainDepartment domain)
        => new()
        {
            DepartmentUuid = domain.Id,
            Name = domain.Name
        };
}