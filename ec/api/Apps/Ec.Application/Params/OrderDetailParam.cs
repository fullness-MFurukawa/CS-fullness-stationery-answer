namespace Ec.Application.Params;

/// <summary>
/// 購入履歴詳細取得の入力値（UC007）
/// </summary>
/// <param name="OrderId">注文識別ID(uuid)</param>
/// <param name="CustomerId">要求元の顧客識別ID(uuid)。認証済みのトークンから取得する</param>
public sealed record OrderDetailParam(Guid OrderId, Guid CustomerId);