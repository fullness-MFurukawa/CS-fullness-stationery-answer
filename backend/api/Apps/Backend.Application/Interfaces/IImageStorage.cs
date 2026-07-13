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

    /// <summary>
    /// 画像を削除する
    /// </summary>
    /// <param name="imageUrl">削除対象の画像の公開URL（SaveAsyncが返した値）</param>
    /// <returns>削除の完了を表すタスク</returns>
    /// <remarks>
    /// 商品保存に失敗した際、保存済みの画像を取り消すために使用する。
    /// 対象が存在しない場合も例外とせず正常終了とする。
    /// </remarks>
    Task DeleteAsync(string imageUrl);
}