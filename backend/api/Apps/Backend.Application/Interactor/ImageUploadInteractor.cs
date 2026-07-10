using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Exceptions;

namespace Backend.Application.Interactor;

/// <summary>
/// 補助:商品画像アップロードのユースケースの実装
/// </summary>
public class ImageUploadInteractor : IImageUploadUsecase
{
    /// <summary>
    /// アップロードを許可する最大バイト数（2MB）
    /// </summary>
    private const long MaxLength = 2 * 1024 * 1024;

    /// <summary>
    /// アップロードを許可する拡張子とMIMEタイプの対応
    /// </summary>
    private static readonly Dictionary<string, string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    /// <summary>
    /// PNGファイルの先頭バイト列
    /// </summary>
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    /// JPEGファイルの先頭バイト列
    /// </summary>
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];

    private readonly IImageStorage _imageStorage;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="imageStorage">画像の保存先</param>
    public ImageUploadInteractor(IImageStorage imageStorage)
    {
        _imageStorage = imageStorage;
    }

    /// <summary>
    /// 画像をアップロードする
    /// </summary>
    /// <param name="param">商品画像アップロードの入力</param>
    /// <returns>保存された画像の公開URL</returns>
    public async Task<string> ExecuteAsync(ImageUploadParam param)
    {
        ArgumentNullException.ThrowIfNull(param);

        if (param.Length <= 0)
            throw new DomainException("画像が指定されていません。");

        if (param.Length > MaxLength)
            throw new DomainException($"画像のサイズが上限（{MaxLength / 1024 / 1024}MB）を超えています。");

        var extension = Path.GetExtension(param.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.TryGetValue(extension, out var expectedContentType))
            throw new DomainException("PNG形式またはJPEG形式の画像を指定してください。");

        if (!string.Equals(param.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("画像の形式と拡張子が一致していません。");

        await ValidateSignatureAsync(param.Content, expectedContentType);

        // 元のファイル名は使用せず、拡張子のみを引き継いだ一意な名前を生成する
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        return await _imageStorage.SaveAsync(param.Content, fileName);
    }

    /// <summary>
    /// 画像の先頭バイト列を検証する
    /// </summary>
    /// <param name="content">画像の内容を読み取るストリーム</param>
    /// <param name="contentType">拡張子から判定したMIMEタイプ</param>
    /// <remarks>
    /// Content-Type ヘッダおよび拡張子は詐称できるため、実体の先頭バイト列を確認する。
    /// 検証後はストリームの位置を先頭へ戻す。
    /// </remarks>
    private static async Task ValidateSignatureAsync(Stream content, string contentType)
    {
        var signature = contentType == "image/png" ? PngSignature : JpegSignature;

        var header = new byte[signature.Length];
        var read = await content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);

        if (read < signature.Length || !header.SequenceEqual(signature))
            throw new DomainException("PNG形式またはJPEG形式の画像を指定してください。");

        content.Position = 0;
    }
}