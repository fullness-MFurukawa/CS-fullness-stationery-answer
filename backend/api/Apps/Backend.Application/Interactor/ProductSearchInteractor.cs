using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC011:商品検索のユースケース実装
/// </summary>
public class ProductSearchInteractor : IProductSearchUsecase
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    public ProductSearchInteractor(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// 商品カテゴリで商品を検索する
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)。指定しない場合はnull(全件取得)</param>
    /// <returns>論理削除を除いた商品の一覧</returns>
    public async Task<IReadOnlyList<Product>> ExecuteAsync(Guid? categoryId)
    {
        if (categoryId is null)
        {
            return await _productRepository.FindAllAsync();
        }

        return await _productRepository.FindByCategoryAsync(categoryId.Value);
    }
}