using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 認証済みの社員アカウントとログインレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスはパスワードハッシュと社員集約を含まないため、ドメインオブジェクトへは復元できない。
/// </remarks>
public class LoginResponseAdapter : IEntityAdapter<LoginResponse, EmployeeAccount>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">担当者ログインのレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">パスワードハッシュと社員集約が失われるため復元できない</exception>
    public EmployeeAccount ToDomain(LoginResponse source)
        => throw new NotSupportedException("レスポンスから社員アカウントへの復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">認証に成功した社員アカウント</param>
    /// <returns>担当者ログインのレスポンス</returns>
    public LoginResponse ToSource(EmployeeAccount domain)
        => new(domain.Name, domain.Employee.Name);
}