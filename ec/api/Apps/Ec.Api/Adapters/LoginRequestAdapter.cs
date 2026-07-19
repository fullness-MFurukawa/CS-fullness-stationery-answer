using Ec.Api.ViewModels.Requests;
using Ec.Application.Params;
using Ec.Domain.Adapters;
namespace Ec.Api.Adapters;

/// <summary>
/// 顧客ログインのリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// 逆方向（入力値からリクエストへの復元）は用途が存在せず未サポートとする。
/// </remarks>
public class LoginRequestAdapter : IEntityAdapter<LoginRequest, CustomerLoginParam>
{
    /// <summary>
    /// リクエストからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">顧客ログインのリクエスト</param>
    /// <returns>顧客ログインの入力値</returns>
    public CustomerLoginParam ToDomain(LoginRequest source)
        => new(source.MailAddress, source.Password);

    /// <summary>
    /// ユースケースの入力値からリクエストへ変換する（未サポート）
    /// </summary>
    /// <param name="domain">顧客ログインの入力値</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public LoginRequest ToSource(CustomerLoginParam domain)
        => throw new NotSupportedException("入力値からリクエストへの変換は使用しません。");
}