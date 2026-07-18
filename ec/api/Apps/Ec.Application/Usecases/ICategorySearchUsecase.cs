using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC003:商品カテゴリ一覧取得のユースケース
/// </summary>
public interface ICategorySearchUsecase
{
    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>商品カテゴリの一覧（カテゴリID順）</returns>
    Task<IReadOnlyList<ProductCategory>> ExecuteAsync();
}