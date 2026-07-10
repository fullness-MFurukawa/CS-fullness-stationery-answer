using System.ComponentModel.DataAnnotations;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC017:担当者ログインのリクエスト
/// </summary>
/// <param name="AccountName">アカウント名</param>
/// <param name="Password">平文のパスワード</param>
public sealed record LoginRequest(
    [property: Required(ErrorMessage = "アカウント名を入力してください")]
    string AccountName,

    [property: Required(ErrorMessage = "パスワードを入力してください")]
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(LoginRequest)} {{ {nameof(AccountName)} = {AccountName} }}";
}