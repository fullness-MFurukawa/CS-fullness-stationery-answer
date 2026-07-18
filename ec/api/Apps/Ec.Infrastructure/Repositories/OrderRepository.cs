using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Ec.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;
using EfOrder = Ec.Infrastructure.Entities.Order;
using EfOrderDetail = Ec.Infrastructure.Entities.OrderDetail;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 注文（注文集約）のリポジトリ実装
/// </summary>
/// <remarks>
/// 管理サービス側と異なり、EC側は注文を新規に生成する。
/// また、参照できるのは自身の注文のみである（UC007）。
/// </remarks>
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
    /// 注文を新規登録(注文明細を含む注文集約を登録)
    /// </summary>
    /// <param name="order">登録する注文</param>
    /// <exception cref="InternalException">関連する顧客・支払い方法・商品が解決できない場合、またはデータベースの登録に失敗した場合</exception>
    /// <remarks>
    /// ドメインの識別子はUUIDだが、テーブルの外部キーは連番(int)である。
    /// そのため、UUIDから対応する連番のIDを引き当ててから挿入する。
    /// このメソッドは購入確定（UC005）のトランザクションの内側で呼ばれ、
    /// 在庫の更新と同一のトランザクションに属する。
    /// </remarks>
    public async Task AddAsync(Order order)
    {
        try
        {
            // UUIDから連番の顧客IDを引き当てる
            var customerId = await _context.Customers
                .Where(c => c.CustomerUuid == order.Customer.Id)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();
            if (customerId == 0)
            {
                throw new InternalException("注文する顧客を解決できませんでした。");
            }

            // 支払い方法・注文ステータスの識別子は、そのまま連番のID
            var entity = new EfOrder
            {
                OrderUuid = order.Id,
                OrderDate = order.OrderDate,
                AmountTotal = order.AmountTotal,
                CustomerId = customerId,
                OrderStatusId = order.Status.Id,
                PaymentMethodId = order.PaymentMethod.Id,
            };

            // 注文明細を組み立てる。商品UUIDから連番の商品IDを引き当てる
            foreach (var detail in order.Details)
            {
                var productId = await _context.Products
                    .Where(p => p.ProductUuid == detail.Product.Id)
                    .Select(p => p.Id)
                    .FirstOrDefaultAsync();
                if (productId == 0)
                {
                    throw new InternalException("注文する商品を解決できませんでした。");
                }

                entity.OrderDetails.Add(new EfOrderDetail
                {
                    ProductId = productId,
                    Count = detail.Count,
                });
            }

            _context.Orders.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("注文の登録に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("注文の登録に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 顧客を指定して注文履歴を取得(UC007)
    /// </summary>
    /// <param name="customerId">顧客識別ID(uuid)</param>
    /// <returns>該当する顧客の注文一覧（注文日時の降順）</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<Order>> FindByCustomerAsync(Guid customerId)
    {
        try
        {
            var entities = await QueryWithIncludes()
                .Where(e => e.Customer.CustomerUuid == customerId)
                .OrderByDescending(e => e.OrderDate)
                .ToListAsync();
            return entities.Select(_orderFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDと顧客を指定して注文を取得(UC007)
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <param name="customerId">顧客識別ID(uuid)</param>
    /// <returns>該当する注文。存在しない、または他の顧客の注文である場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Order?> FindByIdAsync(Guid id, Guid customerId)
    {
        try
        {
            // 注文IDと顧客IDの両方で絞り込む。
            // 他人の注文はこの条件に合致せず、nullが返る
            var entity = await QueryWithIncludes()
                .FirstOrDefaultAsync(e => e.OrderUuid == id && e.Customer.CustomerUuid == customerId);
            return entity is null ? null : _orderFactory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("注文情報の取得に失敗しました。", ex);
        }
    }
}