using Backend.Domain.Models;

namespace Backend.Domain.Repositories;

/// <summary>
/// 社員の参照を担うリポジトリ
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// アカウント未登録の社員を取得
    /// </summary>
    /// <returns>アカウントが未作成の社員一覧</returns>
    Task<IReadOnlyList<Employee>> FindWithoutAccountAsync();

    /// <summary>
    /// 識別IDを指定して社員を取得
    /// </summary>
    /// <param name="id">社員識別ID(uuid)</param>
    /// <returns>該当する社員。存在しない場合はnull</returns>
    Task<Employee?> FindByIdAsync(Guid id);
}