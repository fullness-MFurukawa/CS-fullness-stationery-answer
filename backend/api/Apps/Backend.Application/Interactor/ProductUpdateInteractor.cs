using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC012:商品修正のユースケース実装
/// </summary>
public class ProductUpdateInteractor : IProductUpdateUsecase
{
    private readonly IProductRepository _productRepository;
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public ProductUpdateInteractor(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 既存の商品情報と在庫数を変更する
    /// </summary>
    /// <param name="param">商品修正の入力値</param>
    /// <returns>修正された商品</returns>
    /// <exception cref="NotFoundException">対象の商品または商品カテゴリが存在しない場合</exception>
    /// <exception cref="DomainException">商品名・価格・在庫数が不正な場合</exception>
    public async Task<Product> ExecuteAsync(ProductUpdateParam param)
    {
        // トランザクション境界を制御して商品情報を修正する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 修正対象の商品を取得する
            var current = await _productRepository.FindByIdAsync(param.ProductId)
                ?? throw new NotFoundException("指定された商品は存在しません。");

            // 論理削除済みの商品は修正できない（削除済みという内部状態は漏らさない）
            if (current.IsDeleted)
            {
                throw new NotFoundException("指定された商品は存在しません。");
            }

            // 変更後の商品カテゴリを取得する
            var category = await _productCategoryRepository.FindByIdAsync(param.CategoryId)
                ?? throw new NotFoundException("指定された商品カテゴリが存在しません。");

            // 識別子は維持したまま、新しい値で商品集約を再構築する
            var stock = new ProductStock(current.Stock.Id, param.Quantity);
            var product = new Product(
                current.Id,
                param.Name,
                param.Price,
                param.ImageUrl,
                category,
                stock,
                current.IsDeleted);

            await _productRepository.UpdateAsync(product);

            return product;
        });
    }
}