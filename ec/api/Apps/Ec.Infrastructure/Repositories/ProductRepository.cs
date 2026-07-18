using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Ec.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 商品（商品集約）のリポジトリ実装
/// </summary>
/// <remarks>
/// 管理サービス側と異なり、EC側は商品の登録・修正・削除を行わない。
/// 参照と、購入に伴う在庫の更新のみを担う。
/// いずれの参照も論理削除された商品を除外する。
/// </remarks>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ProductStockAdapter _stockAdapter;
    private readonly ProductFactory _productFactory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="stockAdapter">商品在庫のアダプタ</param>
    /// <param name="productFactory">商品集約を組み立てるファクトリ</param>
    public ProductRepository(
        AppDbContext context,
        ProductStockAdapter stockAdapter,
        ProductFactory productFactory)
    {
        _context = context;
        _stockAdapter = stockAdapter;
        _productFactory = productFactory;
    }

    /// <summary>
    /// すべての有効な商品を取得（論理削除を除く）
    /// </summary>
    /// <returns>商品の一覧（商品ID順）</returns>
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
    /// <returns>該当カテゴリの商品一覧（商品ID順）</returns>
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
    /// 識別IDを指定して有効な商品を取得（論理削除を除く）
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <returns>該当する商品。存在しない、または論理削除されている場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Product?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Products
                .AsNoTracking()
                .Include(e => e.Category)
                .Include(e => e.Stock)
                .FirstOrDefaultAsync(e => e.ProductUuid == id && e.DeleteFlg == 0);
            return entity is null ? null : _productFactory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して有効な商品を取得し、在庫レコードを悲観的ロックする(UC005)
    /// </summary>
    /// <param name="ids">商品識別ID(uuid)の一覧</param>
    /// <returns>該当する有効な商品の一覧。存在しない・論理削除された識別IDは含まれない</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    /// <remarks>
    /// PostgreSQLの FOR UPDATE により、取得した商品の行をトランザクション終了まで
    /// 他のトランザクションから更新できないようロックする。
    /// これにより、二人の顧客が同時に最後の1個を購入して在庫が負数になる事態を防ぐ。
    ///
    /// ロックの取得順序を商品ID(uuid)の昇順に固定している。
    /// 順序を揃えないと、顧客Aが商品1→商品2、顧客Bが商品2→商品1の順にロックを取り、
    /// 互いに相手の解放を待ち続けるデッドロックが起きうる。
    /// </remarks>
    public async Task<IReadOnlyList<Product>> FindByIdsForUpdateAsync(IReadOnlyCollection<Guid> ids)
    {
        try
        {
            // デッドロックを避けるため、ロックを取る順序を一意に定める
            var orderedIds = ids.Distinct().OrderBy(id => id).ToArray();

            // まず対象商品の主キー(連番)を、UUIDから解決する。
            // FOR UPDATE は product_stock の行に対して掛ける（在庫が競合の対象のため）。
            // ここでは主キーを昇順に取得しておき、その順でロックする。
            var products = new List<Entities.Product>();
            foreach (var uuid in orderedIds)
            {
                // FromSql で FOR UPDATE を発行する。
                // product_stock を結合し、在庫行をロックする。
                // 論理削除された商品は対象外とする。
                var entity = await _context.Products
                    .FromSql(
                        $@"SELECT p.* FROM product p
                           JOIN product_stock s ON s.product_id = p.id
                           WHERE p.product_uuid = {uuid} AND p.delete_flg = 0
                           FOR UPDATE OF s")
                    .Include(e => e.Category)
                    .Include(e => e.Stock)
                    .FirstOrDefaultAsync();

                if (entity is not null)
                {
                    products.Add(entity);
                }
            }

            return products.Select(_productFactory.Create).ToList();
        }
        catch (DbException ex)
        {
            throw new InternalException("商品情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 商品の在庫数を更新(UC005)
    /// </summary>
    /// <param name="product">在庫を更新する商品</param>
    /// <exception cref="InternalException">対象の在庫を解決できない場合、またはデータベースの更新に失敗した場合</exception>
    /// <remarks>
    /// 購入に伴う在庫の引き当てに用いる。商品名や価格は更新しない。
    /// このメソッドは購入確定（UC005）のトランザクションの内側で呼ばれ、
    /// FindByIdsForUpdateAsync で取得したロック済みの行を更新する。
    /// </remarks>
    public async Task UpdateStockAsync(Product product)
    {
        try
        {
            // 在庫はUUIDで対象を特定する
            var stockEntity = await _context.ProductStocks
                .FirstOrDefaultAsync(e => e.StockUuid == product.Stock.Id);
            if (stockEntity is null)
            {
                throw new InternalException("更新対象の在庫を解決できませんでした。");
            }
            stockEntity.Quantity = product.Stock.Quantity;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("在庫の更新に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("在庫情報の取得に失敗しました。", ex);
        }
    }
}