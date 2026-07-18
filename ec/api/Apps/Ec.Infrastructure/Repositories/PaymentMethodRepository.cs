using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 支払い方法（マスタ）のリポジトリ実装
/// </summary>
/// <remarks>
/// EC側は購入確認画面のプルダウン（UC005）と、購入確定時の実在確認に参照する。
/// </remarks>
public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _context;
    private readonly PaymentMethodAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="adapter">支払い方法のアダプタ</param>
    public PaymentMethodRepository(AppDbContext context, PaymentMethodAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての支払い方法を取得
    /// </summary>
    /// <returns>支払い方法の一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<PaymentMethod>> FindAllAsync()
    {
        try
        {
            var entities = await _context.PaymentMethods
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ToListAsync();
            return entities.Select(_adapter.ToDomain).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("支払い方法の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// IDを指定して支払い方法を取得
    /// </summary>
    /// <param name="id">支払い方法ID</param>
    /// <returns>該当する支払い方法。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<PaymentMethod?> FindByIdAsync(int id)
    {
        try
        {
            var entity = await _context.PaymentMethods
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("支払い方法の取得に失敗しました。", ex);
        }
    }
}