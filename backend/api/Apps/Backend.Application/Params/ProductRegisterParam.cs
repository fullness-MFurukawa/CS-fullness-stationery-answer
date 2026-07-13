namespace Backend.Application.Params;

/// <summary>
/// UC010:新商品登録の入力値
/// </summary>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">初期在庫数</param>
/// <param name="ImageContent">画像の内容を読み取るストリーム。画像を指定しない場合はnull</param>
/// <param name="ImageFileName">アップロードされた画像の元のファイル名</param>
/// <param name="ImageContentType">アップロードされた画像のMIMEタイプ</param>
/// <param name="ImageLength">アップロードされた画像のバイト数</param>
/// <remarks>
/// 画像はプレゼンテーション層の型（IFormFile）に依存しないよう、
/// ストリームと最小限のメタ情報として受け取る。
/// ImageContent が null の場合、画像なしの商品として登録する。
/// </remarks>
public sealed record ProductRegisterParam(
    string Name,
    int Price,
    Guid CategoryId,
    int Quantity,
    Stream? ImageContent = null,
    string? ImageFileName = null,
    string? ImageContentType = null,
    long ImageLength = 0);