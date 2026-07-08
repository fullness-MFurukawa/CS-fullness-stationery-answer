using Backend.Domain.Models;

namespace Backend.Domain.Repositories;

/// <summary>
/// 社員アカウント（管理者ログイン用）の永続化を担うリポジトリ
/// </summary>
public interface IEmployeeAccountRepository
{
    /// <summary>
    /// アカウント名を指定して社員アカウントを取得
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>該当する社員アカウント。存在しない場合はnull</returns>
    Task<EmployeeAccount?> FindByAccountNameAsync(string accountName);

    /// <summary>
    /// 指定したアカウント名が既に登録されているかを確認
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>登録済みの場合はtrue</returns>
    Task<bool> ExistsByAccountNameAsync(string accountName);

    /// <summary>
    /// 社員アカウントを新規登録
    /// </summary>
    /// <param name="account">登録する社員アカウント</param>
    Task AddAsync(EmployeeAccount account);
}