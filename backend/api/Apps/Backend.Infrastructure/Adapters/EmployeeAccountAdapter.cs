using EfEmployeeAccount = Backend.Infrastructure.Entities.EmployeeAccount;
using DomainEmployeeAccount = Backend.Domain.Models.EmployeeAccount;
using DomainEmployee = Backend.Domain.Models.Employee;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 社員アカウント(EmployeeAccount)とEFエンティティを相互変換するアダプタ
/// 関連（社員）の変換は行わず、変換済みのものを受け取る
/// </summary>
public class EmployeeAccountAdapter
{
    /// <summary>
    /// EFエンティティと変換済みの社員からドメインの社員アカウントを生成
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <param name="employee">変換済みの社員</param>
    /// <returns>ドメインの社員アカウント</returns>
    public DomainEmployeeAccount ToDomain(EfEmployeeAccount source, DomainEmployee employee)
        => new(source.AccountUuid, source.Name, source.Password, employee);

    /// <summary>
    /// ドメインの社員アカウントからEFエンティティへ変換（スカラー項目のみ）
    /// </summary>
    /// <param name="domain">変換元のドメインの社員アカウント</param>
    /// <returns>EFエンティティ</returns>
    public EfEmployeeAccount ToSource(DomainEmployeeAccount domain)
        => new()
        {
            AccountUuid = domain.Id,
            Name = domain.Name,
            Password = domain.Password
        };
}