using System.ComponentModel.DataAnnotations;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC009:担当者アカウント登録のリクエスト
/// </summary>
/// <param name="EmployeeId">アカウントを作成する社員の識別ID(uuid)</param>
/// <param name="AccountName">アカウント名</param>
/// <param name="Password">平文のパスワード</param>
public sealed record EmployeeAccountRegisterRequest(
    [property: Required(ErrorMessage = "社員名を選択してください")]
    Guid? EmployeeId,

    [property: Required(ErrorMessage = "アカウント名を入力してください")]
    [property: StringLength(20, MinimumLength = 5, ErrorMessage = "アカウント名は5～20文字で入力してください")]
    [property: RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "アカウント名は半角英数字で入力してください")]
    string AccountName,

    [property: Required(ErrorMessage = "パスワードを入力してください")]
    [property: StringLength(20, MinimumLength = 5, ErrorMessage = "パスワードは5～20文字で入力してください")]
    [property: RegularExpression("^[a-zA-Z0-9]+$", ErrorMessage = "パスワードは半角英数字で入力してください")]
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(EmployeeAccountRegisterRequest)} {{ " +
           $"{nameof(EmployeeId)} = {EmployeeId}, " +
           $"{nameof(AccountName)} = {AccountName} }}";
}