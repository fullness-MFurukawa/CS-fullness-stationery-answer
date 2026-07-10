namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 社員アカウントのレスポンス
/// </summary>
/// <param name="AccountId">アカウント識別ID(uuid)</param>
/// <param name="AccountName">アカウント名</param>
/// <param name="EmployeeName">紐づく社員名</param>
/// <remarks>
/// パスワード（ハッシュ値）は保持しない。ViewModelにプロパティを設けないことで、
/// レスポンスへ混入する経路そのものを排除する。
/// </remarks>
public sealed record EmployeeAccountResponse(
    Guid AccountId,
    string AccountName,
    string EmployeeName);