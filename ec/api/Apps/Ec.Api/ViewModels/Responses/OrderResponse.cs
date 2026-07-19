namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 注文のレスポンス
/// </summary>
/// <param name="OrderId">注文識別ID(uuid)</param>
/// <param name="OrderDate">注文日時</param>
/// <param name="AmountTotal">合計金額</param>
/// <param name="StatusName">注文ステータス名</param>
/// <param name="PaymentMethodName">支払い方法名</param>
/// <param name="Details">注文明細の一覧</param>
public sealed record OrderResponse(
    Guid OrderId,
    DateTime OrderDate,
    int AmountTotal,
    string StatusName,
    string PaymentMethodName,
    IReadOnlyList<OrderDetailResponse> Details);