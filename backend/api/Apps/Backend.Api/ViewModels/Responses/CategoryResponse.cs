namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 商品カテゴリのレスポンス
/// </summary>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Name">商品カテゴリ名</param>
public sealed record CategoryResponse(
    Guid CategoryId,
    string Name);