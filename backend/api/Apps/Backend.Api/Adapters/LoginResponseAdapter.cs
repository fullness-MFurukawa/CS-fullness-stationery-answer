using Backend.Api.ViewModels.Responses;
using Backend.Application.Results;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// 担当者ログインの実行結果とログインレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスはパスワードハッシュや社員集約を含まず、復元もしないため、逆方向は未サポートとする。
/// </remarks>
public class LoginResponseAdapter : IEntityAdapter<LoginResponse, EmployeeLoginResult>
{
    /// <summary>
    /// レスポンスから実行結果へ変換する（未サポート）
    /// </summary>
    /// <param name="source">担当者ログインのレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">実行結果を復元できないため</exception>
    public EmployeeLoginResult ToDomain(LoginResponse source)
        => throw new NotSupportedException("レスポンスからログイン実行結果への復元は行えません。");

    /// <summary>
    /// 実行結果からレスポンスへ変換する
    /// </summary>
    /// <param name="domain">担当者ログインの実行結果</param>
    /// <returns>担当者ログインのレスポンス</returns>
    public LoginResponse ToSource(EmployeeLoginResult domain)
        => new(domain.Account.Name, domain.Account.Employee.Name, domain.Token.Value);
}