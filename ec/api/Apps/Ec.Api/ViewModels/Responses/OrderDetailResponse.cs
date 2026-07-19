namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 注文明細のレスポンス
/// </summary>
/// <param name="ProductId">商品識別ID(uuid)</param>
/// <param name="ProductName">商品名</param>
/// <param name="Price">単価</param>
/// <param name="Count">注文数量</param>
/// <param name="Subtotal">小計（単価×数量）</param>
public sealed record OrderDetailResponse(
    Guid ProductId,
    string ProductName,
    int Price,
    int Count,
    int Subtotal);