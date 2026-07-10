namespace Backend.Application.Params;

/// <summary>
/// 商品画像アップロードの入力
/// </summary>
/// <param name="Content">画像の内容を読み取るストリーム</param>
/// <param name="FileName">アップロードされた元のファイル名</param>
/// <param name="ContentType">アップロードされたファイルのMIMEタイプ</param>
/// <param name="Length">アップロードされたファイルのバイト数</param>
/// <remarks>
/// プレゼンテーション層の型（IFormFile）に依存しないための入力。
/// FileName は拡張子の判定にのみ使用し、保存先のファイル名には使用しない。
/// </remarks>
public sealed record ImageUploadParam(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);