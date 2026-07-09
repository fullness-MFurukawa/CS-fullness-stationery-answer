
using System.Data.Common;
using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 商品カテゴリのリポジトリ実装
/// </summary>
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
    /// <returns>商品カテゴリの一覧</returns>
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
        catch(DbException ex)
        {
            throw new InternalException("商品カテゴリの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して商品カテゴリを取得
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当する商品カテゴリ。存在しない場合はnull</returns>
    public async Task<ProductCategory?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.ProductCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CategoryUuid == id);
            // 存在しない場合はnullを返す
            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch(Exception ex)
        {
            throw new InternalException("商品カテゴリの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 商品カテゴリを新規登録
    /// </summary>
    /// <param name="category">登録する商品カテゴリ</param>
    public async Task AddAsync(ProductCategory category)
    {
        try
        {
            var entity = _adapter.ToSource(category);

            _context.ProductCategories.Add(entity);
            await _context.SaveChangesAsync();   
        }
        catch(DbException ex)
        {
            throw new InternalException("商品カテゴリの登録に失敗しました。", ex);
        }
    }
}