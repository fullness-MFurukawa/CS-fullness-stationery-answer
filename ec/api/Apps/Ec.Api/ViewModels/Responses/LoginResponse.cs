namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 顧客ログインのレスポンス
/// </summary>
/// <param name="CustomerName">顧客名</param>
/// <param name="AccessToken">アクセストークン（JWT）</param>
/// <remarks>
/// アクセストークンはレスポンスボディで返す。
/// フロントエンド（NextAuth）がトークンを保持し、以降のリクエストで
/// Authorization: Bearer ヘッダーとして送る。
/// トークンは秘密情報のため、ToStringではログへ出力しない。
/// </remarks>
public sealed record LoginResponse(
    string CustomerName,
    string AccessToken)
{
    /// <summary>
    /// アクセストークンがログ等へ出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>アクセストークンを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(LoginResponse)} {{ {nameof(CustomerName)} = {CustomerName} }}";
}