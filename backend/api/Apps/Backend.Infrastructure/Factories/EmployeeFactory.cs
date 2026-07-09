using Backend.Domain.Adapters;
using Backend.Domain.Exceptions;
using Backend.Infrastructure.Adapters;
using EfEmployee = Backend.Infrastructure.Entities.Employee;
using DomainEmployee = Backend.Domain.Models.Employee;

namespace Backend.Infrastructure.Factories;

/// <summary>
/// EFの社員エンティティからドメインの社員集約を組み立てるファクトリ
/// </summary>
public class EmployeeFactory : IAggregateFactory<EfEmployee, DomainEmployee>
{
    private readonly DepartmentAdapter _departmentAdapter;
    private readonly EmployeeAdapter _employeeAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="departmentAdapter">部署のアダプタ</param>
    /// <param name="employeeAdapter">社員のアダプタ</param>
    public EmployeeFactory(DepartmentAdapter departmentAdapter, EmployeeAdapter employeeAdapter)
    {
        _departmentAdapter = departmentAdapter;
        _employeeAdapter = employeeAdapter;
    }

    /// <summary>
    /// EFの社員エンティティ（Department を Include 済み）から社員集約を組み立てる
    /// </summary>
    /// <param name="source">組み立て元のEFエンティティ</param>
    /// <returns>社員集約（ドメイン）</returns>
    /// <exception cref="DomainException">Department が未ロードの場合</exception>
    public DomainEmployee Create(EfEmployee source)
    {
        // 関連（部署）が未ロードなら組み立て不可
        if (source.Department is null)
        {
            throw new DomainException("部署が読み込まれていません。");
        }

        // 部署をAdapterで変換
        var department = _departmentAdapter.ToDomain(source.Department);

        // 変換結果を使って社員集約を組み立てる
        return _employeeAdapter.ToDomain(source, department);
    }
}