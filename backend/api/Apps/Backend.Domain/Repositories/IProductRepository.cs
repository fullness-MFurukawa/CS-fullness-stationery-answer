using Backend.Domain.Models;

namespace Backend.Domain.Repositories;

/// <summary>
/// 商品(商品集約)の永続化を担うリポジトリ
/// </summary>
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
    /// 商品を新規登録(在庫を含む商品集約を登録)
    /// </summary>
    /// <param name="product">登録する商品</param>
    Task AddAsync(Product product);

    /// <summary>
    /// 商品情報を更新
    /// </summary>
    /// <param name="product">更新する商品</param>
    Task UpdateAsync(Product product);

    /// <summary>
    /// 商品を削除（論理削除）
    /// </summary>
    /// <param name="id">削除対象の商品識別ID(uuid)</param>
    Task DeleteByIdAsync(Guid id);

    /// <summary>
    /// 有効な商品の件数を取得(論理削除を除く)
    /// </summary>
    /// <returns>有効な商品の件数</returns>
    Task<int> CountAsync();
}