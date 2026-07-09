using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC010:新商品登録のユースケース
/// </summary>
public interface IProductRegisterUsecase
{
    /// <summary>
    /// 新しい商品を在庫とともに登録する
    /// </summary>
    /// <param name="param">新商品登録の入力値</param>
    /// <returns>登録された商品</returns>
    Task<Product> ExecuteAsync(ProductRegisterParam param);
}