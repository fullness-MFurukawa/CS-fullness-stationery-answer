namespace Backend.Application.Params;

/// <summary>
/// UC012:商品修正の入力値
/// </summary>
/// <param name="ProductId">修正対象の商品識別ID(uuid)</param>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">在庫数</param>
/// <param name="ImageContent">新しい画像の内容を読み取るストリーム。画像を変更しない場合はnull</param>
/// <param name="ImageFileName">アップロードされた画像の元のファイル名</param>
/// <param name="ImageContentType">アップロードされた画像のMIMEタイプ</param>
/// <param name="ImageLength">アップロードされた画像のバイト数</param>
/// <remarks>
/// ImageContent が null の場合、画像は変更せず既存の画像URLを維持する。
/// 画像を指定した場合は差し替え、更新成功後に古い画像を削除する。
/// </remarks>
public sealed record ProductUpdateParam(
    Guid ProductId,
    string Name,
    int Price,
    Guid CategoryId,
    int Quantity,
    Stream? ImageContent = null,
    string? ImageFileName = null,
    string? ImageContentType = null,
    long ImageLength = 0);