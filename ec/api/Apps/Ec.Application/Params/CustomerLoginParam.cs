namespace Ec.Application.Params;

/// <summary>
/// 顧客ログインの入力値（UC002）
/// </summary>
/// <param name="MailAddress">メールアドレス</param>
/// <param name="Password">パスワード（平文）</param>
public sealed record CustomerLoginParam(string MailAddress, string Password);