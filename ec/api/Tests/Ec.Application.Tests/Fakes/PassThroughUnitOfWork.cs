using Ec.Application.Interfaces;
namespace Ec.Application.Tests.Fakes;

/// <summary>
/// トランザクション制御を行わず、処理をそのまま実行するテスト用のユニットオブワーク
/// </summary>
public sealed class PassThroughUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// トランザクションを開始する（何もしない）
    /// </summary>
    public Task BeginTransactionAsync() => Task.CompletedTask;

    /// <summary>
    /// トランザクションをコミットする（何もしない）
    /// </summary>
    public Task CommitAsync() => Task.CompletedTask;

    /// <summary>
    /// トランザクションをロールバックする（何もしない）
    /// </summary>
    public Task RollbackAsync() => Task.CompletedTask;

    /// <summary>
    /// 処理をそのまま実行する
    /// </summary>
    /// <typeparam name="TResult">処理の戻り値の型</typeparam>
    /// <param name="action">実行する処理</param>
    /// <returns>処理の戻り値</returns>
    public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action) => action();

    /// <summary>
    /// 処理をそのまま実行する
    /// </summary>
    /// <param name="action">実行する処理</param>
    public Task ExecuteInTransactionAsync(Func<Task> action) => action();
}