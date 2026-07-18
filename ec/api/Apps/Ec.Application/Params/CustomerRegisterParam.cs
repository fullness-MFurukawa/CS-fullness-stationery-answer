namespace Ec.Application.Params;

/// <summary>
/// 顧客アカウント登録の入力値（UC001）
/// </summary>
/// <param name="Name">顧客名</param>
/// <param name="NameKana">顧客名カナ</param>
/// <param name="Address1">住所1</param>
/// <param name="Address2">住所2</param>
/// <param name="PhoneNumber">電話番号</param>
/// <param name="MailAddress">メールアドレス</param>
/// <param name="Username">アカウント名</param>
/// <param name="Password">パスワード（平文）</param>
public sealed record CustomerRegisterParam(
    string Name,
    string NameKana,
    string Address1,
    string? Address2,
    string PhoneNumber,
    string MailAddress,
    string Username,
    string Password);