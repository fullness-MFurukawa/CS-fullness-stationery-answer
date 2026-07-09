namespace Backend.Application.Params;

/// <summary>
/// UC016:注文ステータス更新の入力値
/// </summary>
/// <param name="OrderId">更新対象の注文識別ID(uuid)</param>
/// <param name="OrderStatusId">新しい注文ステータスID</param>
public sealed record OrderStatusUpdateParam(
    Guid OrderId,
    int OrderStatusId);