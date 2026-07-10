using Backend.Application.Interfaces;
using Backend.Infrastructure.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Storage;

/// <summary>
/// ローカルファイルシステムへ画像を保存する実装
/// </summary>
/// <remarks>
/// 第1段階の実装。第2段階では AzureBlobImageStorage へ差し替える。
/// </remarks>
public class LocalImageStorage : IImageStorage
{
    private readonly ImageStorageOptions _options;
    private readonly string _absoluteRootPath;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="options">画像保存の設定</param>
    /// <param name="environment">ホスト環境</param>
    public LocalImageStorage(IOptions<ImageStorageOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _absoluteRootPath = Path.Combine(environment.ContentRootPath, _options.RootPath);
    }

    /// <summary>
    /// 画像を保存し、公開URLを返す
    /// </summary>
    /// <param name="content">画像の内容を読み取るストリーム</param>
    /// <param name="fileName">保存先のファイル名</param>
    /// <returns>保存された画像の公開URL</returns>
    /// <exception cref="InternalException">ファイルの書き込みに失敗した場合</exception>
    public async Task<string> SaveAsync(Stream content, string fileName)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        try
        {
            Directory.CreateDirectory(_absoluteRootPath);

            var filePath = Path.Combine(_absoluteRootPath, fileName);

            await using var fileStream = new FileStream(
                filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

            await content.CopyToAsync(fileStream);

            return $"{_options.PublicBaseUrl.TrimEnd('/')}{_options.RequestPath}/{fileName}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InternalException("画像の保存に失敗しました。", ex);
        }
    }
}