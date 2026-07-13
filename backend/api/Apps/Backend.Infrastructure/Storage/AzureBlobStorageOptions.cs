namespace Backend.Infrastructure.Storage;

/// <summary>
/// Azure Blob Storageへの画像保存の設定
/// </summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    /// 設定ファイル上のセクション名
    /// </summary>
    public const string SectionName = "AzureBlobStorage";

    /// <summary>
    /// ストレージアカウントへの接続文字列
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 画像を格納するコンテナ名
    /// </summary>
    public string ContainerName { get; set; } = "images";

    /// <summary>
    /// コンテナ内で画像を配置する仮想ディレクトリ(プレフィックス)
    /// </summary>
    /// <remarks>
    /// Blobにフォルダの概念はないが、ファイル名にプレフィックスを付けることでポータル上ではフォルダのように扱える
    /// </remarks>
    public string Prefix { get; set; } = "products";
}