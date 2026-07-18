using System.Data.Common;

using Ec.Application.Interfaces;
using Ec.Infrastructure.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ec.Infrastructure.Contexts;

/// <summary>
/// AppDbContext のトランザクション境界を制御するユニットオブワーク
/// </summary>
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// トランザクションを開始する
    /// </summary>
    /// <exception cref="InternalException">既に開始済みの場合、または開始に失敗した場合</exception>
    public async Task BeginTransactionAsync()
    {
        if (_transaction is not null)
        {
            throw new InternalException("トランザクションは既に開始されています。");
        }

        try
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
        catch (DbException ex)
        {
            throw new InternalException("トランザクションの開始に失敗しました。", ex);
        }
    }

    /// <summary>
    /// トランザクションをコミットする
    /// </summary>
    /// <exception cref="InternalException">未開始の場合、またはコミットに失敗した場合</exception>
    public async Task CommitAsync()
    {
        if (_transaction is null)
        {
            throw new InternalException("トランザクションが開始されていません。");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        catch (DbException ex)
        {
            throw new InternalException("トランザクションのコミットに失敗しました。", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// トランザクションをロールバックする
    /// </summary>
    /// <exception cref="InternalException">ロールバックに失敗した場合</exception>
    public async Task RollbackAsync()
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        catch (DbException ex)
        {
            throw new InternalException("トランザクションのロールバックに失敗しました。", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// 保持しているトランザクションを破棄する
    /// </summary>
    private async ValueTask DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// 非同期にリソースを解放する
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// トランザクション内で処理を実行し、成功時はコミット、例外時はロールバックする
    /// 既にトランザクションが開始されている場合は、その境界に参加する
    /// </summary>
    /// <typeparam name="TResult">処理の戻り値の型</typeparam>
    /// <param name="action">トランザクション内で実行する処理</param>
    /// <returns>処理の戻り値</returns>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action)
    {
        // 既に外側でトランザクションが開始されている場合は、その境界に参加する
        // （テストで外側からトランザクションを張るケースを想定）
        if (_context.Database.CurrentTransaction is not null)
        {
            return await action();
        }

        await BeginTransactionAsync();

        try
        {
            var result = await action();
            await CommitAsync();
            return result;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// トランザクション内で処理を実行し、成功時はコミット、例外時はロールバックする
    /// 既にトランザクションが開始されている場合は、その境界に参加する
    /// </summary>
    /// <param name="action">トランザクション内で実行する処理</param>
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await action();
            return true;
        });
    }
}