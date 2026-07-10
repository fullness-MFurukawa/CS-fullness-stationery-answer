using Backend.Domain.Adapters;
using Backend.Domain.Exceptions;
using Backend.Infrastructure.Adapters;

using DomainEmployeeAccount = Backend.Domain.Models.EmployeeAccount;
using EfEmployeeAccount = Backend.Infrastructure.Entities.EmployeeAccount;

namespace Backend.Infrastructure.Factories;

/// <summary>
/// EFの社員アカウントエンティティからドメインのアカウント集約を組み立てるファクトリ
/// </summary>
public class EmployeeAccountFactory : IAggregateFactory<EfEmployeeAccount, DomainEmployeeAccount>
{
    private readonly EmployeeFactory _employeeFactory;
    private readonly EmployeeAccountAdapter _employeeAccountAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeFactory">社員集約を組み立てるファクトリ</param>
    /// <param name="employeeAccountAdapter">社員アカウントのアダプタ</param>
    public EmployeeAccountFactory(
        EmployeeFactory employeeFactory,
        EmployeeAccountAdapter employeeAccountAdapter)
    {
        _employeeFactory = employeeFactory;
        _employeeAccountAdapter = employeeAccountAdapter;
    }

    /// <summary>
    /// EFの社員アカウントエンティティ（Employee・Department を Include 済み）からアカウント集約を組み立てる
    /// </summary>
    /// <param name="source">組み立て元のEFエンティティ</param>
    /// <returns>アカウント集約（ドメイン）</returns>
    /// <exception cref="DomainException">Employee が未ロードの場合</exception>
    public DomainEmployeeAccount Create(EfEmployeeAccount source)
    {
        // 関連（社員）が未ロードなら組み立て不可
        if (source.Employee is null)
        {
            throw new DomainException("社員が読み込まれていません。");
        }

        // 社員（＋部署）を EmployeeFactory で組み立て
        var employee = _employeeFactory.Create(source.Employee);

        // 変換結果を使ってアカウント集約を組み立てる
        return _employeeAccountAdapter.ToDomain(source, employee);
    }
}