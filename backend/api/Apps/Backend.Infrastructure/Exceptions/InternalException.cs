namespace Backend.Infrastructure.Exceptions;

/// <summary>
/// インフラストラクチャ層で発生した技術的例外を表す例外
/// EF Coreやデータベースから送出された例外をラップし、上位層へ伝播させる
/// </summary>
public class InternalException : Exception
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーの内容</param>
    public InternalException(string message)
        : base(message){}

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーの内容</param>
    /// <param name="innerException">この例外の原因となった内部例外</param>
    public InternalException(string message, Exception innerException)
        : base(message, innerException) {}
}