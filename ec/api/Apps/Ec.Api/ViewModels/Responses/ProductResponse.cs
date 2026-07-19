namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 商品のレスポンス
/// </summary>
/// <remarks>
/// EC側は論理削除された商品を返さないため、IsDeletedは含めない。
/// </remarks>
/// <param name="ProductId">商品識別ID(uuid)</param>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="ImageUrl">画像URL</param>
/// <param name="Quantity">在庫数</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="CategoryName">商品カテゴリ名</param>
public sealed record ProductResponse(
    Guid ProductId,
    string Name,
    int Price,
    string? ImageUrl,
    int Quantity,
    Guid CategoryId,
    string CategoryName);