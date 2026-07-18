namespace Ec.Application.Exceptions;

/// <summary>
/// 登録しようとしたリソースが既に存在する場合にスローされる例外
/// アカウント名やカテゴリ名の重複を検知した際、アプリケーション層で送出する
/// </summary>
public class ExistsException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">重複していることを伝えるメッセージ</param>
    public ExistsException(string message) : base(message) { }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">重複していることを伝えるメッセージ</param>
    /// <param name="innerException">この例外の原因となった内部例外</param>
    public ExistsException(string message, Exception innerException) : base(message, innerException) { }
}