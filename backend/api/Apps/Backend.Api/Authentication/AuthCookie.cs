namespace Backend.Api.Authentication;

/// <summary>
/// 認証トークンを格納するCookieの定義
/// </summary>
/// <remarks>
/// Cookie名は AuthController（設定・失効）と Program.cs（読み出し）の双方で使用するため、
/// 定数として一元管理する。
/// </remarks>
public static class AuthCookie
{
    /// <summary>
    /// 認証トークンを格納するCookieの名前
    /// </summary>
    /// <remarks>
    /// __Host- 接頭辞により、Secure かつ Path=/ かつ Domain属性なしであることをブラウザが強制する。
    /// サブドメインからのCookie上書き（session fixation）を防止する。
    /// </remarks>
    public const string Name = "__Host-auth";

    /// <summary>
    /// 認証Cookieを設定するためのオプションを生成する
    /// </summary>
    /// <param name="expiresAt">Cookieの有効期限</param>
    /// <returns>Cookieオプション</returns>
    public static CookieOptions Create(DateTimeOffset expiresAt) => new()
    {
        HttpOnly = true,                    // JavaScriptから読み取り不可 (XSS対策)
        Secure = true,                      // HTTPS通信でのみ送信
        SameSite = SameSiteMode.Strict,     // クロスサイトのリクエストでは送信しない (CSRF緩和)
        Path = "/",                         // __Host- 接頭辞の要件
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
    public static CookieOptions CreateForDelete() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/"
    };
}