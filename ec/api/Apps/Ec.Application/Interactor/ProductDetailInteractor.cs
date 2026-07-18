using Ec.Application.Exceptions;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC004:商品詳細取得のユースケース実装
/// </summary>
public class ProductDetailInteractor : IProductDetailUsecase
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    public ProductDetailInteractor(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// 識別IDを指定して商品を取得する
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <returns>該当する商品</returns>
    /// <exception cref="NotFoundException">商品が存在しない、または論理削除されている場合</exception>
    public async Task<Product> ExecuteAsync(Guid id)
    {
        // リポジトリが論理削除を除外するため、
        // ここで取得できた商品は必ず販売中である。
        return await _productRepository.FindByIdAsync(id)
            ?? throw new NotFoundException("指定された商品が存在しません。");
    }
}