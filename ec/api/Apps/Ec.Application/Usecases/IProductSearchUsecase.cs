using Ec.Application.Params;
using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC003:商品検索のユースケース
/// </summary>
public interface IProductSearchUsecase
{
    /// <summary>
    /// 商品を検索する
    /// </summary>
    /// <param name="param">商品検索の入力値</param>
    /// <returns>該当する商品の一覧（商品ID順）</returns>
    Task<IReadOnlyList<Product>> ExecuteAsync(ProductSearchParam param);
}