using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 注文ステータス（マスタ）のリポジトリ実装
/// </summary>
/// <remarks>
/// EC側は購入確定（UC005）で初期ステータス（注文済）を取得するために参照するのみ。
/// </remarks>
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