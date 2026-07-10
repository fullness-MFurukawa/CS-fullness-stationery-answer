namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 注文明細のレスポンス
/// </summary>
/// <param name="ProductName">商品名</param>
/// <param name="Price">単価</param>
/// <param name="Count">注文数</param>
/// <param name="Subtotal">小計（単価×注文数）</param>
public sealed record OrderDetailResponse(
    string ProductName,
    int Price,
    int Count,
    int Subtotal);