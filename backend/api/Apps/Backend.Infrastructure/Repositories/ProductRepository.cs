using System.Data.Common;

using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Backend.Infrastructure.Factories;

using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 商品（商品集約）のリポジトリ実装
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ProductAdapter _productAdapter;
    private readonly ProductStockAdapter _stockAdapter;
    private readonly ProductFactory _productFactory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="productAdapter">商品のアダプタ</param>
    /// <param name="stockAdapter">商品在庫のアダプタ</param>
    /// <param name="productFactory">商品集約を組み立てるファクトリ</param>
    public ProductRepository(
        AppDbContext context,
        ProductAdapter productAdapter,
        ProductStockAdapter stockAdapter,
        ProductFactory productFactory)
    {
        _context = context;
        _productAdapter = productAdapter;
        _stockAdapter = stockAdapter;
        _productFactory = productFactory;
    }

    /// <summary>
    /// すべての有効な商品を取得（論理削除を除く）
    /// </summary>
    /// <returns>商品の一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<Product>> FindAllAsync()
    {
        try
        {
            var entities = await _context.Products
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Stock)
                .Where(e => e.DeleteFlg == 0)
                .OrderBy(e => e.Id)
                .ToListAsync();

            return entities.Select(_productFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 指定カテゴリに属する有効な商品を取得（論理削除を除く）
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当カテゴリの商品一覧</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<IReadOnlyList<Product>> FindByCategoryAsync(Guid categoryId)
    {
        try
        {
            var entities = await _context.Products
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Stock)
                .Where(e => e.DeleteFlg == 0 && e.Category.CategoryUuid == categoryId)
                .OrderBy(e => e.Id)
                .ToListAsync();

            return entities.Select(_productFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して商品を取得
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <returns>該当する商品。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Product?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Products
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Stock)
                .FirstOrDefaultAsync(e => e.ProductUuid == id);

            return entity is null ? null : _productFactory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 商品を新規登録（在庫を含む商品集約を登録）
    /// </summary>
    /// <param name="product">登録する商品</param>
    /// <exception cref="DomainException">指定された商品カテゴリが存在しない場合</exception>
    /// <exception cref="InternalException">データベースへの登録に失敗した場合</exception>
    public async Task AddAsync(Product product)
    {
        try
        {
            // 商品カテゴリの外部キーを uuid から解決する
            var categoryEntity = await _context.ProductCategories
                .FirstOrDefaultAsync(e => e.CategoryUuid == product.Category.Id);

            if (categoryEntity is null)
            {
                throw new DomainException("指定された商品カテゴリが存在しません。");
            }

            var entity = _productAdapter.ToSource(product);
            entity.ProductCategoryId = categoryEntity.Id;

            // 在庫は商品集約の一部なので、ナビゲーション経由で同時に登録する
            entity.Stock = _stockAdapter.ToSource(product.Stock);

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("商品の登録に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品カテゴリの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 商品情報を更新
    /// </summary>
    /// <param name="product">更新する商品</param>
    /// <exception cref="DomainException">対象の商品または商品カテゴリが存在しない場合</exception>
    /// <exception cref="InternalException">データベースの更新に失敗した場合</exception>
    public async Task UpdateAsync(Product product)
    {
        try
        {
            // 更新対象を追跡状態で取得する（在庫も同時更新するため Include）
            var entity = await _context.Products
                .Include(e => e.Stock)
                .FirstOrDefaultAsync(e => e.ProductUuid == product.Id);

            if (entity is null)
            {
                throw new DomainException("指定された商品は存在しません。");
            }

            // 商品カテゴリの外部キーを uuid から解決する
            var categoryEntity = await _context.ProductCategories
                .FirstOrDefaultAsync(e => e.CategoryUuid == product.Category.Id);

            if (categoryEntity is null)
            {
                throw new DomainException("指定された商品カテゴリが存在しません。");
            }

            entity.Name = product.Name;
            entity.Price = product.Price;
            entity.ImageUrl = product.ImageUrl;
            entity.DeleteFlg = product.IsDeleted ? 1 : 0;
            entity.ProductCategoryId = categoryEntity.Id;

            if (entity.Stock is not null)
            {
                entity.Stock.Quantity = product.Stock.Quantity;
            }

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("商品の更新に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 商品を削除（論理削除。delete_flg を 1 に更新する）
    /// </summary>
    /// <param name="id">削除対象の商品識別ID(uuid)</param>
    /// <exception cref="DomainException">対象の商品が存在しない場合</exception>
    /// <exception cref="InternalException">データベースの更新に失敗した場合</exception>
    public async Task DeleteByIdAsync(Guid id)
    {
        try
        {
            // 削除対象を追跡状態で取得する
            var entity = await _context.Products
                .FirstOrDefaultAsync(e => e.ProductUuid == id);

            if (entity is null)
            {
                throw new DomainException("指定された商品は存在しません。");
            }

            // 物理削除は行わず、削除フラグを立てる
            entity.DeleteFlg = 1;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("商品の削除に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 有効な商品の件数を取得(論理削除を除く)
    /// </summary>
    /// <returns>有効な商品の件数</returns>
    /// <exception cref="InternalException">データベースアクセスに失敗した場合</exception>
    public async Task<int> CountAsync()
    {
        try
        {
            return await _context.Products
                .Where(p => p.DeleteFlg == 0)
                .CountAsync();
        }
        catch (Exception ex) when (ex is not InternalException)
        {
            throw new InternalException("商品の件数取得に失敗しました。", ex);
        }
    }
}