namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 注文ステータスのレスポンス
/// </summary>
/// <param name="OrderStatusId">注文ステータスID</param>
/// <param name="Name">注文ステータス名</param>
public sealed record OrderStatusResponse(
    int OrderStatusId,
    string Name);