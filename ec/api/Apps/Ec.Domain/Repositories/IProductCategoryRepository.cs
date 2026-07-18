using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 商品カテゴリの永続化を担うリポジトリ
/// </summary>
/// <remarks>
/// EC側は商品の絞り込み（UC003）のために参照するのみで、登録は行わない。
/// カテゴリの登録は管理サービス側（UC014）の責務である。
/// </remarks>
public interface IProductCategoryRepository
{
    /// <summary>
    /// すべての商品カテゴリを取得
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    /// <remarks>カテゴリ検索画面（FP006）のプルダウンに表示する。</remarks>
    Task<IReadOnlyList<ProductCategory>> FindAllAsync();

    /// <summary>
    /// 識別IDを指定して商品カテゴリを取得
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <returns>該当する商品カテゴリ。存在しない場合はnull</returns>
    /// <remarks>
    /// 検索条件に指定されたカテゴリが実在するかの確認に用いる。
    /// 存在しないカテゴリを指定された場合、
    /// 「該当0件」ではなく404を返すことで、誤りを利用者へ伝える。
    /// </remarks>
    Task<ProductCategory?> FindByIdAsync(Guid id);
}