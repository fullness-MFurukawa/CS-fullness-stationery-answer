namespace Ec.Application.Results;

/// <summary>
/// 認証済みの利用者に発行するアクセストークン
/// </summary>
/// <param name="Value">トークン文字列</param>
/// <param name="ExpiresAt">有効期限</param>
public sealed record AccessToken(
    string Value,
    DateTimeOffset ExpiresAt)
{
    /// <summary>
    /// トークン文字列がログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>トークン文字列を含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(AccessToken)} {{ {nameof(ExpiresAt)} = {ExpiresAt:O} }}";
}