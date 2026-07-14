namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 担当者ログインのレスポンス
/// </summary>
/// <param name="AccountName">アカウント名</param>
/// <param name="EmployeeName">担当者名</param>
/// <param name="AccessToken">アクセストークン（JWT）</param>
/// <remarks>
/// アクセストークンは、HttpOnly Cookie に加えてレスポンスボディでも返す。
/// Cookie はブラウザからの Cookie 認証（Swagger 等）に、
/// ボディのトークンは NextAuth 等がトークンを保持して Authorization ヘッダで送る用途に使う。
/// トークンは秘密情報のため、ToString ではログへ出力しない。
/// </remarks>
public sealed record LoginResponse(
    string AccountName,
    string EmployeeName,
    string AccessToken)
{
    /// <summary>
    /// アクセストークンがログ等へ出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>アクセストークンを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(LoginResponse)} {{ {nameof(AccountName)} = {AccountName}, " +
           $"{nameof(EmployeeName)} = {EmployeeName} }}";
}