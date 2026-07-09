using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC014:商品カテゴリ登録のユースケース
/// </summary>
public interface ICategoryRegisterUsecase
{
    /// <summary>
    /// 新しい商品カテゴリを登録する
    /// </summary>
    /// <param name="name">商品カテゴリ名</param>
    /// <returns>登録された商品カテゴリ</returns>
    Task<ProductCategory> ExecuteAsync(string name);
}