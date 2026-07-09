namespace Backend.Application.Params;

/// <summary>
/// UC015:購入履歴検索の入力値
/// </summary>
/// <param name="OrderDate">購入日。指定しない場合はnull</param>
/// <param name="CustomerAccountName">顧客アカウント名。指定しない場合はnull</param>
public sealed record OrderHistorySearchParam(
    DateOnly? OrderDate,
    string? CustomerAccountName);