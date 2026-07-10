namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 社員のレスポンス
/// </summary>
/// <param name="EmployeeId">社員識別ID(uuid)</param>
/// <param name="Name">社員名</param>
/// <param name="NameKana">社員名カナ</param>
/// <param name="DepartmentName">所属部署名</param>
public sealed record EmployeeResponse(
    Guid EmployeeId,
    string Name,
    string? NameKana,
    string DepartmentName);