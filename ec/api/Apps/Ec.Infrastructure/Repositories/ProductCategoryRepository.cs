using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 商品カテゴリのリポジトリ実装
/// </summary>
/// <remarks>
/// EC側は商品の絞り込み（UC003）のために参照するのみで、登録は行わない。
/// </remarks>
public class ProductCategoryRepository : IProductCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly ProductCategoryAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="adapter">商品カテゴリのアダプタ</param>
    public ProductCategoryRepository(AppDbContext context, ProductCategoryAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての商品カテゴリを取得
    /// </summary>
    /// <returns>商品カテゴリの一覧（カテゴリID順）</returns>
    /// <exception cref="InternalException">データベースアクセスに失敗した場合</exception>
    public async Task<IReadOnlyList<ProductCategory>> FindAllAsync()
    {
        try
        {
            var entities = await _context.ProductCategories
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .ToListAsync();
            return entities.Select(_adapter.ToDomain).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("商品カテゴリの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して商品カテゴリを取得
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当する商品カテゴリ。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースアクセスに失敗した場合</exception>
    public async Task<ProductCategory?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.ProductCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CategoryUuid == id);
            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品カテゴリの取得に失敗しました。", ex);
        }
    }
}