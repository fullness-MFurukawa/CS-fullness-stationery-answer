namespace Backend.Domain.Exceptions;

/// <summary>
/// 業務ルール違反を表すドメイン例外の基底クラス
/// 在庫不足や重複登録など、ドメイン（業務）の制約に反した場合にスローする
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">利用者や上位層に伝える業務エラーの内容</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">業務エラーの内容</param>
    /// <param name="innerException">この例外の原因となった内部例外</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}