using System.Data.Common;
using Backend.Application.Interfaces;
using Backend.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Backend.Infrastructure.Contexts;

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
}