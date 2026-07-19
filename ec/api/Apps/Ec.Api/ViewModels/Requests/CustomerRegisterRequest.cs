using System.ComponentModel.DataAnnotations;
namespace Ec.Api.ViewModels.Requests;

/// <summary>
/// UC001:顧客アカウント登録のリクエスト
/// </summary>
/// <remarks>
/// バリデーションは画面仕様(FP003)に対応する。
/// 文字種などドメインの不変条件を超える制約は、この入力チェックで弾く。
/// </remarks>
/// <param name="Name">氏名</param>
/// <param name="NameKana">氏名カナ</param>
/// <param name="Address1">住所1</param>
/// <param name="Address2">住所2</param>
/// <param name="PhoneNumber">電話番号</param>
/// <param name="MailAddress">メールアドレス</param>
/// <param name="Username">アカウント名</param>
/// <param name="Password">平文のパスワード</param>
public sealed record CustomerRegisterRequest(
    [Required(ErrorMessage = "氏名を入力してください")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "氏名は2〜20文字で入力してください")]
    string Name,

    [Required(ErrorMessage = "氏名カナを入力してください")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "氏名カナは2〜20文字で入力してください")]
    [RegularExpression("^[ァ-ヶー]+$", ErrorMessage = "氏名カナは全角カナで入力してください")]
    string NameKana,

    [Required(ErrorMessage = "住所1を入力してください")]
    [StringLength(100, ErrorMessage = "住所1は100文字以内で入力してください")]
    string Address1,

    [StringLength(100, ErrorMessage = "住所2は100文字以内で入力してください")]
    string? Address2,

    [Required(ErrorMessage = "電話番号を入力してください")]
    [RegularExpression(@"^\d{2,4}-\d{2,4}-\d{4}$", ErrorMessage = "電話番号は「市外局番-市内局番-番号」形式で入力してください")]
    string PhoneNumber,

    [Required(ErrorMessage = "メールアドレスを入力してください")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "メールアドレスは4〜100文字で入力してください")]
    [EmailAddress(ErrorMessage = "正しいメールアドレス形式で入力してください")]
    string MailAddress,

    [Required(ErrorMessage = "アカウント名を入力してください")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "アカウント名は5〜20文字で入力してください")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "アカウント名は半角英数字で入力してください")]
    string Username,

    [Required(ErrorMessage = "パスワードを入力してください")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "パスワードは5〜20文字で入力してください")]
    [RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "パスワードは半角英数字で入力してください")]
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(CustomerRegisterRequest)} {{ {nameof(MailAddress)} = {MailAddress}, " +
           $"{nameof(Username)} = {Username} }}";
}