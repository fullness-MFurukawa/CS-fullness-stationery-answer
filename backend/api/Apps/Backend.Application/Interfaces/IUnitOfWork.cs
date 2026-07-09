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
}