namespace Ec.Infrastructure.Security;

/// <summary>
/// JWTの生成に使用する構成情報
/// </summary>
/// <remarks>
/// EC側（顧客向け）のJWT設定。管理サービス側とは別の署名鍵・発行者・対象者を用いる。
/// これにより、顧客のトークンで管理サービスのAPIを呼び出せないようにする。
/// 構成ファイルでも "Jwt" ではなく "CustomerJwt" セクションから読む。
/// </remarks>
public class JwtOptions
{
    /// <summary>
    /// 構成ファイル上のセクション名
    /// </summary>
    public const string SectionName = "CustomerJwt";

    /// <summary>
    /// トークンの発行者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// トークンの想定利用者
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 署名に使用する秘密鍵（HMAC-SHA256のため32バイト以上が必要）
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// トークンの有効期限（分）
    /// </summary>
    public int ExpiresMinutes { get; set; } = 30;
}