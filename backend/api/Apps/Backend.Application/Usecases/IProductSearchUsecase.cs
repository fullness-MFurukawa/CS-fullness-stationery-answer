using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC011:商品検索のユースケース
/// </summary>
public interface IProductSearchUsecase
{
    /// <summary>
    /// 商品カテゴリで商品を検索する
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)。指定しない場合はnull(全件取得)</param>
    /// <returns>論理削除を除いた商品の一覧</returns>
    Task<IReadOnlyList<Product>> ExecuteAsync(Guid? categoryId);
}