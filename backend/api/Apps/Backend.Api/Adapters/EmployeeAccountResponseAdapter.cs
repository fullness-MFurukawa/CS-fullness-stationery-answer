using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 社員アカウントのドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスはパスワードハッシュと社員集約を含まないため、ドメインオブジェクトへは復元できない。
/// </remarks>
public class EmployeeAccountResponseAdapter : IEntityAdapter<EmployeeAccountResponse, EmployeeAccount>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">社員アカウントのレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">パスワードハッシュと社員集約が失われるため復元できない</exception>
    public EmployeeAccount ToDomain(EmployeeAccountResponse source)
        => throw new NotSupportedException("レスポンスから社員アカウントへの復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの社員アカウント</param>
    /// <returns>社員アカウントのレスポンス</returns>
    public EmployeeAccountResponse ToSource(EmployeeAccount domain)
        => new(
            domain.Id,
            domain.Name,
            domain.Employee.Name);
}