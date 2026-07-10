namespace Backend.Api.Authentication;

/// <summary>
/// 認証トークンを格納するCookieの設定
/// </summary>
public class AuthCookieOptions
{
    /// <summary>
    /// 設定ファイル上のセクション名
    /// </summary>
    public const string SectionName = "AuthCookie";

    /// <summary>
    /// 認証トークンを格納するCookieの名前
    /// </summary>
    public string Name { get; set; } = "auth";

    /// <summary>
    /// HTTPS通信でのみ送信するかどうか
    /// </summary>
    /// <remarks>
    /// HTTPで動作させる開発環境では false とする。
    /// HTTPSで公開する本番環境では true とし、トークンの傍受を防ぐ。
    /// </remarks>
    public bool Secure { get; set; }
}