using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC013:商品削除のユースケース実装
/// </summary>
public class ProductDeleteInteractor : IProductDeleteUsecase
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public ProductDeleteInteractor(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定した商品を削除する（論理削除）
    /// </summary>
    /// <param name="productId">削除対象の商品識別ID(uuid)</param>
    /// <returns>削除された商品</returns>
    /// <exception cref="NotFoundException">対象の商品が存在しない、または既に削除済みの場合</exception>
    public async Task<Product> ExecuteAsync(Guid productId)
    {
        // トランザクション境界を制御して商品を削除する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 削除対象の商品を取得する
            var current = await _productRepository.FindByIdAsync(productId)
                ?? throw new NotFoundException("指定された商品は存在しません。");

            // 既に論理削除済みの商品は削除できない
            if (current.IsDeleted)
            {
                throw new NotFoundException("指定された商品は存在しません。");
            }

            await _productRepository.DeleteByIdAsync(productId);

            // 削除後の状態を反映した商品を返す（完了画面で商品名を表示するため）
            return new Product(
                current.Id,
                current.Name,
                current.Price,
                current.ImageUrl,
                current.Category,
                current.Stock,
                isDeleted: true);
        });
    }
}