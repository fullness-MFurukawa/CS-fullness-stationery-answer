namespace Backend.Application.Exceptions;

/// <summary>
/// 指定されたリソースが存在しない場合にスローされる例外
/// リポジトリがnullを返した際、アプリケーション層で未検出として送出する
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">未検出であることを伝えるメッセージ</param>
    public NotFoundException(string message) : base(message) { }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">未検出であることを伝えるメッセージ</param>
    /// <param name="innerException">この例外の原因となった内部例外</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}