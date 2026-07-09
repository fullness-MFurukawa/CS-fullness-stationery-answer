using Backend.Domain.Models;

namespace Backend.Application.Results;

/// <summary>
/// UC017:担当者ログインの実行結果
/// </summary>
/// <param name="Account">認証に成功した社員アカウント</param>
/// <param name="Token">発行されたアクセストークン</param>
public sealed record EmployeeLoginResult(
    EmployeeAccount Account,
    AccessToken Token);