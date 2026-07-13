using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Backend.Application.Interfaces;
using Backend.Infrastructure.Exceptions;

using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Storage;
/// <summary>
/// Azure Blob Storageへ画像を保存する実装
/// </summary>
/// <remarks>
/// アップロードされた商品画像をAzure Blob Storageに保存し、その公開URLを返す
/// 保存先をローカルのファイルシステムからクラウドストレージへ移すための実装で、
/// 同じIImageStorageを実装する LocalImageStorage と入れ替えて使用する
/// どちらを使うかはDIコンテナの登録で切り替えるため、画像アップロードのユースケース側は変更する必要がない
/// </remarks>
public class AzureBlobImageStorage : IImageStorage
{
    private readonly AzureBlobStorageOptions _options;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">Azure Blob Storageの設定</param>
    public AzureBlobImageStorage(IOptions<AzureBlobStorageOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 画像を保存し、公開URLを返す
    /// </summary>
    /// <param name="content">画像の内容を読み取るストリーム</param>
    /// <param name="fileName">保存先のファイル名</param>
    /// <returns>保存された画像の公開URL</returns>
    /// <exception cref="InternalException">Blob Storageへの保存に失敗した場合</exception>
    public async Task<string> SaveAsync(Stream content, string fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            var containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);

            // コンテナが存在しない場合は作成する（BLOB単位の匿名読み取りを許可）
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            // 既存コンテナの場合も匿名読み取りを保証する（CreateIfNotExistsは新規作成時のみレベルを設定するため）
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            // プレフィックスを付けてフォルダのように配置する
            var blobName = string.IsNullOrEmpty(_options.Prefix)
                ? fileName
                : $"{_options.Prefix}/{fileName}";

            var blobClient = containerClient.GetBlobClient(blobName);

            // 画像のMIMEタイプを設定する（ブラウザが正しく表示するために必要）
            var headers = new BlobHttpHeaders
            {
                ContentType = GetContentType(fileName)
            };

            await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = headers });

            return blobClient.Uri.ToString();
        }
        catch (Exception ex) when (ex is not InternalException)
        {
            throw new InternalException("画像の保存に失敗しました。", ex);
        }
    }

    /// <summary>
    /// ファイル名の拡張子からMIMEタイプを判定する
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns>MIMEタイプ</returns>
    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }


    /// <summary>
    /// 画像を削除する
    /// </summary>
    /// <param name="imageUrl">削除対象の画像の公開URL（SaveAsyncが返した値）</param>
    /// <returns>削除の完了を表すタスク</returns>
    /// <remarks>
    /// 商品保存に失敗した際、保存済みの画像を取り消すために使用する
    /// 対象が存在しない場合も例外とせず正常終了とする
    /// </remarks>
    public async Task DeleteAsync(string imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

        try
        {
            var containerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);

            // 公開URLのパスからBlob名（例: products/xxxx.png）を取り出す
            var uri = new Uri(imageUrl);
            var blobName = uri.AbsolutePath
                .TrimStart('/')
                .Substring(_options.ContainerName.Length + 1);

            var blobClient = containerClient.GetBlobClient(blobName);

            // 対象が存在しない場合も例外とせず正常終了とする
            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception ex) when (ex is not InternalException)
        {
            throw new InternalException("画像の削除に失敗しました。", ex);
        }
    }
}