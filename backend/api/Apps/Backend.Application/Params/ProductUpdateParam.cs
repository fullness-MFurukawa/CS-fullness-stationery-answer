namespace Backend.Application.Params;

/// <summary>
/// UC012:商品修正の入力値
/// </summary>
/// <param name="ProductId">修正対象の商品識別ID(uuid)</param>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="ImageUrl">画像URL</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">在庫数</param>
public sealed record ProductUpdateParam(
    Guid ProductId,
    string Name,
    int Price,
    string? ImageUrl,
    Guid CategoryId,
    int Quantity);