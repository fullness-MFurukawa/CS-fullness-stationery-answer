namespace Backend.Application.Params;

/// <summary>
/// UC017:担当者ログインの入力値
/// </summary>
/// <param name="AccountName">アカウント名</param>
/// <param name="Password">平文のパスワード</param>
public sealed record EmployeeLoginParam(
    string AccountName,
    string Password)
{
    /// <summary>
    /// 平文パスワードがログ等に出力されないよう文字列表現を上書きする
    /// </summary>
    /// <returns>パスワードを含まない文字列表現</returns>
    public override string ToString()
        => $"{nameof(EmployeeLoginParam)} {{ {nameof(AccountName)} = {AccountName} }}";
}