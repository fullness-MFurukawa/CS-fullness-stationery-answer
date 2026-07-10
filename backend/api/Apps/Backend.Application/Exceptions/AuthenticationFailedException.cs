namespace Backend.Application.Exceptions;

/// <summary>
/// 認証に失敗した場合にスローされる例外
/// アカウント名の誤りとパスワードの誤りを区別せず、同一のメッセージで送出する
/// </summary>
public class AuthenticationFailedException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">認証失敗を伝えるメッセージ</param>
    public AuthenticationFailedException(string message) : base(message) { }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">認証失敗を伝えるメッセージ</param>
    /// <param name="innerException">この例外の原因となった内部例外</param>
    public AuthenticationFailedException(string message, Exception innerException) : base(message, innerException) { }
}