using Microsoft.Extensions.Options;

namespace Backend.Api.Authentication;

/// <summary>
/// 認証トークンを格納するCookieの生成
/// </summary>
/// <remarks>
/// Cookieの属性は設定時と削除時で一致させる必要があるため、生成処理をこのクラスへ集約する。
/// </remarks>
public class AuthCookie
{
    private readonly AuthCookieOptions _options;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">認証Cookieの設定</param>
    public AuthCookie(IOptions<AuthCookieOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 認証トークンを格納するCookieの名前
    /// </summary>
    public string Name => _options.Name;

    /// <summary>
    /// 認証Cookieを設定するためのオプションを生成する
    /// </summary>
    /// <param name="expiresAt">Cookieの有効期限</param>
    /// <returns>Cookieオプション</returns>
    public CookieOptions Create(DateTimeOffset expiresAt) => new()
    {
        HttpOnly = true,                    // JavaScriptから読み取り不可 (XSS対策)
        Secure = _options.Secure,           // HTTPS通信でのみ送信
        SameSite = SameSiteMode.Strict,     // クロスサイトのリクエストでは送信しない (CSRF緩和)
        Path = "/",
        Expires = expiresAt
    };

    /// <summary>
    /// 認証Cookieを失効させるためのオプションを生成する
    /// </summary>
    /// <returns>Cookieオプション</returns>
    /// <remarks>
    /// 削除時の属性は設定時と一致させる必要がある。
    /// 一致しない場合、ブラウザは別のCookieとみなし削除されない。
    /// </remarks>
    public CookieOptions CreateForDelete() => new()
    {
        HttpOnly = true,
        Secure = _options.Secure,
        SameSite = SameSiteMode.Strict,
        Path = "/"
    };
}