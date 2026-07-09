namespace Backend.Application.Params;

/// <summary>
/// UC010:新商品登録の入力値
/// </summary>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="ImageUrl">画像URL</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">初期在庫数</param>
public sealed record ProductRegisterParam(
    string Name,
    int Price,
    string? ImageUrl,
    Guid CategoryId,
    int Quantity);