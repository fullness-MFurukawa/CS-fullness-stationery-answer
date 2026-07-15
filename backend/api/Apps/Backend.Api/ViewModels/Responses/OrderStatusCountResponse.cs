namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 注文ステータスごとの注文件数のレスポンス
/// </summary>
/// <param name="OrderStatusId">注文ステータスID</param>
/// <param name="Name">注文ステータス名</param>
/// <param name="Count">該当する注文の件数</param>
public sealed record OrderStatusCountResponse(
    int OrderStatusId,
    string Name,
    int Count);