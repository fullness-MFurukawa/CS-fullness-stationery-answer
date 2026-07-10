namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 担当者ログインのレスポンス
/// </summary>
/// <param name="AccountName">アカウント名</param>
/// <param name="EmployeeName">担当者名</param>
/// <remarks>
/// アクセストークンはHttpOnly Cookieで返すため、レスポンスボディには含めない。
/// JavaScriptから読めないようにすることで、XSSによるトークン窃取を防ぐ。
/// </remarks>
public sealed record LoginResponse(
    string AccountName,
    string EmployeeName);