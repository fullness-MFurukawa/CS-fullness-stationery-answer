using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC017:担当者ログインのユースケース
/// </summary>
public interface IEmployeeLoginUsecase
{
    /// <summary>
    /// アカウント名とパスワードで認証し、認証済みの社員アカウントを返す
    /// </summary>
    /// <param name="param">担当者ログインの入力値</param>
    /// <returns>認証に成功した社員アカウント</returns>
    Task<EmployeeAccount> ExecuteAsync(EmployeeLoginParam param);
}