namespace Backend.Infrastructure.Storage;

/// <summary>
/// ローカルファイルシステムへの画像保存の設定
/// </summary>
public class ImageStorageOptions
{
    /// <summary>
    /// 設定ファイル上のセクション名
    /// </summary>
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// 画像を保存する物理ディレクトリ（コンテンツルートからの相対パス）
    /// </summary>
    public string RootPath { get; set; } = "wwwroot/images/products";

    /// <summary>
    /// 画像を公開するURLのパス
    /// </summary>
    public string RequestPath { get; set; } = "/images/products";

    /// <summary>
    /// 画像を公開するURLのオリジン
    /// </summary>
    /// <remarks>
    /// SPAとAPIのオリジンが異なるため、相対URLではなく絶対URLを返す。
    /// </remarks>
    public string PublicBaseUrl { get; set; } = string.Empty;
}