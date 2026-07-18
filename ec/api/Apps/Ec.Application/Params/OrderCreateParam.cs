namespace Ec.Application.Params;

/// <summary>
/// 購入確定の入力値（UC005）
/// </summary>
/// <param name="CustomerId">注文する顧客の識別ID(uuid)。認証済みのトークンから取得する</param>
/// <param name="PaymentMethodId">支払い方法の識別ID</param>
/// <param name="Items">注文する商品と数量の組</param>
public sealed record OrderCreateParam(
    Guid CustomerId,
    int PaymentMethodId,
    IReadOnlyList<OrderItemParam> Items);

/// <summary>
/// 注文する商品1件分の入力値
/// </summary>
/// <param name="ProductId">商品の識別ID(uuid)</param>
/// <param name="Count">注文数量</param>
public sealed record OrderItemParam(Guid ProductId, int Count);