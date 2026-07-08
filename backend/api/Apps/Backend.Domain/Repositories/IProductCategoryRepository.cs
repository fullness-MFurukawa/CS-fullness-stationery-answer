using Backend.Domain.Models;
namespace Backend.Domain.Repositories;

/// <summary>
/// 商品カテゴリの永続化を担うリポジトリ
/// </summary>
public interface IProductCategoryRepository
{
    /// <summary>
    /// すべての商品カテゴリを取得
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    Task<IReadOnlyList<ProductCategory>> FindAllAsync();

    /// <summary>
    /// 識別IDを指定して商品カテゴリを取得
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当する商品カテゴリ。存在しない場合はnull</returns>
    Task<ProductCategory?> FindByIdAsync(Guid id);

    /// <summary>
    /// 商品カテゴリを新規登録
    /// </summary>
    /// <param name="category">登録する商品カテゴリ</param>
    Task AddAsync(ProductCategory category);
}