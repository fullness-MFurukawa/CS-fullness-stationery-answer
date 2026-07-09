using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// 商品カテゴリ一覧取得のユースケース実装
/// </summary>
public class CategorySearchInteractor : ICategorySearchUsecase
{
    private readonly IProductCategoryRepository _productCategoryRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    public CategorySearchInteractor(IProductCategoryRepository productCategoryRepository)
    {
        _productCategoryRepository = productCategoryRepository;
    }

    /// <summary>
    /// すべての商品カテゴリを取得する
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    public async Task<IReadOnlyList<ProductCategory>> ExecuteAsync()
    {
        return await _productCategoryRepository.FindAllAsync();
    }
}