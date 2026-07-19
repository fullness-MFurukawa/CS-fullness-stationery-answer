using System.ComponentModel.DataAnnotations;
namespace Ec.Api.ViewModels.Requests;

/// <summary>
/// UC002:顧客ログインのリクエスト
/// </summary>
/// <param name="MailAddress">メールアドレス</param>
/// <param name="Password">平文のパスワード</param>
public sealed record LoginRequest(
    [Required(ErrorMessage = "メールアドレスを入力してください")]
    string MailAddress,
    [Required(ErrorMessage = "パスワードを入力してください")]
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(LoginRequest)} {{ {nameof(MailAddress)} = {MailAddress} }}";
}