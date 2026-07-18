using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC004:商品詳細取得のユースケース
/// </summary>
public interface IProductDetailUsecase
{
    /// <summary>
    /// 識別IDを指定して商品を取得する
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <returns>該当する商品</returns>
    Task<Product> ExecuteAsync(Guid id);
}