namespace Backend.Application.Interfaces;

/// <summary>
/// 画像の保存先を抽象化するインターフェイス
/// </summary>
/// <remarks>
/// 第1段階ではローカルファイルシステムへ保存し、
/// 第2段階でAzure Blob Storageへの保存に差し替える。
/// </remarks>
public interface IImageStorage
{
    /// <summary>
    /// 画像を保存し、公開URLを返す
    /// </summary>
    /// <param name="content">画像の内容を読み取るストリーム</param>
    /// <param name="fileName">保存先のファイル名</param>
    /// <returns>保存された画像の公開URL</returns>
    Task<string> SaveAsync(Stream content, string fileName);
}