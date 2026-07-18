using Ec.Application.Exceptions;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC003:商品検索のユースケース実装
/// </summary>
public class ProductSearchInteractor : IProductSearchUsecase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    public ProductSearchInteractor(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
    }

    /// <summary>
    /// 商品を検索する
    /// </summary>
    /// <param name="param">商品検索の入力値</param>
    /// <returns>該当する商品の一覧（商品ID順）</returns>
    /// <exception cref="NotFoundException">指定されたカテゴリが存在しない場合</exception>
    public async Task<IReadOnlyList<Product>> ExecuteAsync(ProductSearchParam param)
    {
        // カテゴリ未指定なら全商品を返す
        if (param.CategoryId is null)
        {
            return await _productRepository.FindAllAsync();
        }

        // 指定されたカテゴリが実在するかを確認する。
        // 存在しないカテゴリで「該当0件」を返すと、
        // 「商品がない」のか「カテゴリがない」のか利用者が区別できない。
        // 誤りを伝えるため、存在しなければ404とする。
        var category = await _productCategoryRepository.FindByIdAsync(param.CategoryId.Value)
            ?? throw new NotFoundException("指定された商品カテゴリが存在しません。");

        return await _productRepository.FindByCategoryAsync(category.Id);
    }
}