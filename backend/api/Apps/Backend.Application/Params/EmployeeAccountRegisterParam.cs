namespace Backend.Application.Params;

/// <summary>
/// UC009:担当者アカウント登録の入力値
/// </summary>
/// <param name="EmployeeId">アカウントを作成する社員の識別ID(uuid)</param>
/// <param name="AccountName">アカウント名</param>
/// <param name="Password">平文のパスワード</param>
public sealed record EmployeeAccountRegisterParam(
    Guid EmployeeId,
    string AccountName,
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(EmployeeAccountRegisterParam)} {{ " +
           $"{nameof(EmployeeId)} = {EmployeeId}, " +
           $"{nameof(AccountName)} = {AccountName} }}";
}