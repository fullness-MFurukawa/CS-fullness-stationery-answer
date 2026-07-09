using Backend.Application.Params;
using Backend.Application.Results;

namespace Backend.Application.Usecases;

/// <summary>
/// UC017:担当者ログインのユースケース
/// </summary>
public interface IEmployeeLoginUsecase
{
    /// <summary>
    /// アカウント名とパスワードで認証し、認証済みアカウントとアクセストークンを返す
    /// </summary>
    /// <param name="param">担当者ログインの入力値</param>
    /// <returns>担当者ログインの実行結果</returns>
    Task<EmployeeLoginResult> ExecuteAsync(EmployeeLoginParam param);
}