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
    private readonly IImageUploadUsecase _imageUploadUsecase;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    /// <param name="imageUploadUsecase">商品画像アップロードのユースケース</param>
    /// <param name="imageStorage">画像の保存先（差し替え・取り消しに使用）</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public ProductUpdateInteractor(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        IImageUploadUsecase imageUploadUsecase,
        IImageStorage imageStorage,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _imageUploadUsecase = imageUploadUsecase;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 既存の商品情報と在庫数を変更する
    /// </summary>
    /// <param name="param">商品修正の入力値</param>
    /// <returns>修正された商品</returns>
    /// <exception cref="NotFoundException">対象の商品または商品カテゴリが存在しない場合</exception>
    /// <exception cref="DomainException">商品名・価格・在庫数・画像が不正な場合</exception>
    public async Task<Product> ExecuteAsync(ProductUpdateParam param)
    {
        // 新しい画像があれば先に保存し、公開URLを得る（トランザクションの外で行う）
        string? newImageUrl = null;
        if (param.ImageContent is not null)
        {
            newImageUrl = await _imageUploadUsecase.ExecuteAsync(
                new ImageUploadParam(param.ImageContent, param.ImageFileName!, param.ImageContentType!, param.ImageLength));
        }

        // 更新成功後に削除する、不要になった画像のURL
        string? obsoleteImageUrl = null;

        try
        {
            var product = await _unitOfWork.ExecuteInTransactionAsync(async () =>
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

                // 画像の扱いを決める
                string? imageUrl;
                if (newImageUrl is not null)
                {
                    // 新しい画像を指定した場合は差し替え、古い画像は更新成功後に削除する
                    imageUrl = newImageUrl;
                    obsoleteImageUrl = current.ImageUrl;
                }
                else if (param.RemoveImage)
                {
                    // 画像の削除を指定した場合は画像なしにし、既存の画像は更新成功後に削除する
                    imageUrl = null;
                    obsoleteImageUrl = current.ImageUrl;
                }
                else
                {
                    // 指定がない場合は既存の画像を維持する
                    imageUrl = current.ImageUrl;
                }

                // 識別子は維持したまま、新しい値で商品集約を再構築する
                var stock = new ProductStock(current.Stock.Id, param.Quantity);
                var updated = new Product(
                    current.Id,
                    param.Name,
                    param.Price,
                    imageUrl,
                    category,
                    stock,
                    current.IsDeleted);

                await _productRepository.UpdateAsync(updated);
                return updated;
            });

            // 更新が確定した後に、不要になった画像を削除する（孤児を残さない）
            if (obsoleteImageUrl is not null)
            {
                try
                {
                    await _imageStorage.DeleteAsync(obsoleteImageUrl);
                }
                catch
                {
                    // 古い画像の削除に失敗しても、更新自体は成功しているため握りつぶす
                    // 残った画像は孤児として後で掃除する想定
                }
            }

            return product;
        }
        catch
        {
            // 更新に失敗した場合、新しく保存した画像を取り消す（既存の画像はそのまま残す）
            if (newImageUrl is not null)
            {
                await _imageStorage.DeleteAsync(newImageUrl);
            }
            throw;
        }
    }
}