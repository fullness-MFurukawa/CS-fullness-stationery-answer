using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// 商品カテゴリ一覧取得のユースケース
/// BP006（商品検索）・BP012（新商品登録）・BP009（商品修正）のカテゴリプルダウン用
/// </summary>
public interface ICategorySearchUsecase
{
    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    Task<IReadOnlyList<ProductCategory>> ExecuteAsync();
}