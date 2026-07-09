using System.Data.Common;
using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 注文ステータス（マスタ）のリポジトリ実装
/// </summary>
public class OrderStatusRepository : IOrderStatusRepository
{
    private readonly AppDbContext _context;
    private readonly OrderStatusAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="adapter">注文ステータスのアダプタ</param>
    public OrderStatusRepository(AppDbContext context, OrderStatusAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての注文ステータスを取得
    /// </summary>
    /// <returns>注文ステータスの一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<OrderStatus>> FindAllAsync()
    {
        try
        {
            var entities = await _context.OrderStatuses
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ToListAsync();

            return entities.Select(_adapter.ToDomain).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("注文ステータスの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// IDを指定して注文ステータスを取得
    /// </summary>
    /// <param name="id">注文ステータスID</param>
    /// <returns>該当する注文ステータス。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<OrderStatus?> FindByIdAsync(int id)
    {
        try
        {
            var entity = await _context.OrderStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("注文ステータスの取得に失敗しました。", ex);
        }
    }
}