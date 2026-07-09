namespace Backend.Application.Interfaces;

/// <summary>
/// トランザクション境界を制御するユニットオブワーク
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// トランザクションを開始する
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// トランザクションをコミットする
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// トランザクションをロールバックする
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// トランザクション内で処理を実行し、成功時はコミット、例外時はロールバックする
    /// 既にトランザクションが開始されている場合は、その境界に参加する
    /// </summary>
    /// <typeparam name="TResult">処理の戻り値の型</typeparam>
    /// <param name="action">トランザクション内で実行する処理</param>
    /// <returns>処理の戻り値</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action);

    /// <summary>
    /// トランザクション内で処理を実行し、成功時はコミット、例外時はロールバックする
    /// 既にトランザクションが開始されている場合は、その境界に参加する
    /// </summary>
    /// <param name="action">トランザクション内で実行する処理</param>
    Task ExecuteInTransactionAsync(Func<Task> action);
}