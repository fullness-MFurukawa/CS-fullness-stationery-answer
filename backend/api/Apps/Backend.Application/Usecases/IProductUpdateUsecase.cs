using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC012:商品修正のユースケース
/// </summary>
public interface IProductUpdateUsecase
{
    /// <summary>
    /// 既存の商品情報と在庫数を変更する
    /// </summary>
    /// <param name="param">商品修正の入力値</param>
    /// <returns>修正された商品</returns>
    Task<Product> ExecuteAsync(ProductUpdateParam param);
}