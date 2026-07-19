using Ec.Api.ViewModels.Responses;
using Ec.Application.Results;
using Ec.Domain.Adapters;
namespace Ec.Api.Adapters;

/// <summary>
/// 顧客ログインの実行結果とログインレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは顧客集約を含まず復元もしないため、逆方向は未サポートとする。
/// </remarks>
public class LoginResponseAdapter : IEntityAdapter<LoginResponse, CustomerLoginResult>
{
    /// <summary>
    /// レスポンスから実行結果へ変換する（未サポート）
    /// </summary>
    /// <param name="source">顧客ログインのレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">実行結果を復元できないため</exception>
    public CustomerLoginResult ToDomain(LoginResponse source)
        => throw new NotSupportedException("レスポンスからログイン実行結果への復元は行えません。");

    /// <summary>
    /// 実行結果からレスポンスへ変換する
    /// </summary>
    /// <param name="domain">顧客ログインの実行結果</param>
    /// <returns>顧客ログインのレスポンス</returns>
    public LoginResponse ToSource(CustomerLoginResult domain)
        => new(domain.Customer.Name, domain.Token.Value);
}