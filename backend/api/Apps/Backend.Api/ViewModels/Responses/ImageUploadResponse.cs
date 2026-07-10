namespace Backend.Api.ViewModels.Responses;

/// <summary>
/// 商品画像アップロードのレスポンス
/// </summary>
/// <param name="ImageUrl">保存された画像の公開URL</param>
/// <remarks>
/// このURLを商品登録・商品修正のリクエストに載せて送信する。
/// </remarks>
public sealed record ImageUploadResponse(string ImageUrl);