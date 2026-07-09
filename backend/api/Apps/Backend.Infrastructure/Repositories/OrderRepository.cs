using System.Data.Common;
using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Backend.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;
using EfOrder = Backend.Infrastructure.Entities.Order;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 注文（注文集約）のリポジトリ実装
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly OrderFactory _orderFactory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="orderFactory">注文集約を組み立てるファクトリ</param>
    public OrderRepository(AppDbContext context, OrderFactory orderFactory)
    {
        _context = context;
        _orderFactory = orderFactory;
    }

    /// <summary>
    /// 注文集約の組み立てに必要な関連を Include した参照用クエリを返す
    /// </summary>
    /// <returns>Include済みの注文クエリ</returns>
    private IQueryable<EfOrder> QueryWithIncludes()
        => _context.Orders
            .AsNoTracking()
            .Include(e => e.Customer)
            .Include(e => e.OrderStatus)
            .Include(e => e.PaymentMethod)
            .Include(e => e.OrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Category)
            .Include(e => e.OrderDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Stock);

    /// <summary>
    /// すべての注文を取得（新しい順）
    /// </summary>
    /// <returns>注文の一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<Order>> FindAllAsync()
    {
        try
        {
            var entities = await QueryWithIncludes()
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return entities.Select(_orderFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 購入日・顧客アカウント名で注文を検索
    /// </summary>
    /// <param name="orderDate">購入日。指定しない場合はnull</param>
    /// <param name="customerAccountName">顧客アカウント名。指定しない場合はnull</param>
    /// <returns>条件に一致する注文の一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<Order>> SearchAsync(DateOnly? orderDate, string? customerAccountName)
    {
        try
        {
            var query = QueryWithIncludes();

            if (orderDate is not null)
            {
                // 購入日の 00:00:00 以上、翌日 00:00:00 未満で絞り込む
                var from = orderDate.Value.ToDateTime(TimeOnly.MinValue);
                var to = from.AddDays(1);
                query = query.Where(e => e.OrderDate >= from && e.OrderDate < to);
            }

            if (!string.IsNullOrWhiteSpace(customerAccountName))
            {
                query = query.Where(e => e.Customer.Username == customerAccountName);
            }

            var entities = await query
                .OrderByDescending(e => e.Id)
                .ToListAsync();

            return entities.Select(_orderFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して注文を取得
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <returns>該当する注文。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Order?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await QueryWithIncludes()
                .FirstOrDefaultAsync(e => e.OrderUuid == id);

            return entity is null ? null : _orderFactory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 注文ステータスを更新
    /// </summary>
    /// <param name="orderId">注文識別ID(uuid)</param>
    /// <param name="status">新しい注文ステータス</param>
    /// <exception cref="InternalException">更新対象の注文を解決できない場合、またはデータベースの更新に失敗した場合</exception>
    public async Task UpdateStatusAsync(Guid orderId, OrderStatus status)
    {
        try
        {
            // 更新対象を追跡状態で取得する
            // ※ 存在確認はApplication層で実施済み。ここでの未検出は想定外
            var entity = await _context.Orders
                .FirstOrDefaultAsync(e => e.OrderUuid == orderId);

            if (entity is null)
            {
                throw new InternalException("更新対象の注文を解決できませんでした。注文が同時に削除された可能性があります。");
            }

            // 注文ステータスは同一性が int のIDなので、そのまま外部キーに設定できる
            entity.OrderStatusId = status.Id;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("注文ステータスの更新に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }
}