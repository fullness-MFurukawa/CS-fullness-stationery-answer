using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 商品(商品集約)の永続化を担うリポジトリ
/// </summary>
/// <remarks>
/// 管理サービス側と異なり、EC側は商品の登録・修正・削除を行わない。
/// 参照と、購入に伴う在庫の更新のみを担う。
/// </remarks>
public interface IProductRepository
{
    /// <summary>
    /// すべての有効な商品を取得(論理削除を除く)
    /// </summary>
    /// <returns>商品の一覧</returns>
    Task<IReadOnlyList<Product>> FindAllAsync();

    /// <summary>
    /// 指定カテゴリに属する有効な商品を取得(論理削除を除く)
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当カテゴリの商品一覧</returns>
    Task<IReadOnlyList<Product>> FindByCategoryAsync(Guid categoryId);

    /// <summary>
    /// 識別IDを指定して商品を取得
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <returns>該当する商品。存在しない場合はnull</returns>
    Task<Product?> FindByIdAsync(Guid id);

    /// <summary>
    /// 識別IDを指定して商品を取得し、在庫レコードを悲観的ロックする(UC005)
    /// </summary>
    /// <param name="ids">商品識別ID(uuid)の一覧</param>
    /// <returns>該当する商品の一覧。存在しない識別IDは結果に含まれない</returns>
    /// <remarks>
    /// 購入確定時、在庫を減らす前に対象の在庫レコードをロックする。
    /// ロックしない場合、二人の顧客が同時に最後の1個を購入すると、
    /// 双方が「在庫あり」と判定して在庫が負数になりうる。
    ///
    /// このメソッドはトランザクションの内側で呼び出さなければならない。
    /// ロックはトランザクションの終了まで保持される。
    ///
    /// 複数の商品をロックする場合、ロックを取得する順序は実装側で一意に定める。
    /// 顧客Aが商品1→商品2の順に、顧客Bが商品2→商品1の順にロックを取ると、
    /// 互いに相手の解放を待ち続けるデッドロックが起きる。
    /// </remarks>
    Task<IReadOnlyList<Product>> FindByIdsForUpdateAsync(IReadOnlyCollection<Guid> ids);

    /// <summary>
    /// 商品の在庫数を更新(UC005)
    /// </summary>
    /// <param name="product">在庫を更新する商品</param>
    /// <remarks>
    /// 購入に伴う在庫の引き当てに用いる。
    /// 商品名や価格は更新しない（管理サービス側の責務であるため）。
    /// </remarks>
    Task UpdateStockAsync(Product product);
}