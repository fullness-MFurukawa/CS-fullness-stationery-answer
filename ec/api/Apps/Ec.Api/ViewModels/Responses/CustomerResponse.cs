namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 顧客のレスポンス
/// </summary>
/// <remarks>
/// パスワード（ハッシュ値）は秘密情報のため含めない。
/// 登録完了画面(FP005)で表示する情報に絞る。
/// </remarks>
/// <param name="CustomerId">顧客識別ID(uuid)</param>
/// <param name="Name">氏名</param>
/// <param name="MailAddress">メールアドレス</param>
/// <param name="Username">アカウント名</param>
public sealed record CustomerResponse(
    Guid CustomerId,
    string Name,
    string MailAddress,
    string Username);