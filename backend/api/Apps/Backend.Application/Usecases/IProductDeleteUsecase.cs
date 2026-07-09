using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC013:商品削除のユースケース
/// </summary>
public interface IProductDeleteUsecase
{
    /// <summary>
    /// 指定した商品を削除する（論理削除）
    /// </summary>
    /// <param name="productId">削除対象の商品識別ID(uuid)</param>
    /// <returns>削除された商品</returns>
    Task<Product> ExecuteAsync(Guid productId);
}