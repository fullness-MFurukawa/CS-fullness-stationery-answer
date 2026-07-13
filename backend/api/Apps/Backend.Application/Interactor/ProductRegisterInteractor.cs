using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC010:新商品登録のユースケース実装
/// </summary>
public class ProductRegisterInteractor : IProductRegisterUsecase
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
    /// <param name="imageStorage">画像の保存先（登録失敗時の取り消しに使用）</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public ProductRegisterInteractor(
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
    /// 新しい商品を在庫とともに登録する
    /// </summary>
    /// <param name="param">新商品登録の入力値</param>
    /// <returns>登録された商品</returns>
    /// <exception cref="NotFoundException">指定された商品カテゴリが存在しない場合</exception>
    /// <exception cref="DomainException">商品名・価格・在庫数・画像が不正な場合</exception>
    public async Task<Product> ExecuteAsync(ProductRegisterParam param)
    {
        // 画像があれば先に保存し、公開URLを得る（トランザクションの外で行う）
        string? imageUrl = null;
        if (param.ImageContent is not null)
        {
            imageUrl = await _imageUploadUsecase.ExecuteAsync(
                new ImageUploadParam(param.ImageContent, param.ImageFileName!, param.ImageContentType!, param.ImageLength));
        }

        try
        {
            // トランザクション境界を制御して商品を登録する
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 商品カテゴリを取得する（商品集約の構築に必要）
                var category = await _productCategoryRepository.FindByIdAsync(param.CategoryId)
                    ?? throw new NotFoundException("指定された商品カテゴリが存在しません。");

                // 在庫を含む商品集約を構築する（入力値の妥当性はドメインの不変条件が担保する）
                var stock = new ProductStock(Guid.NewGuid(), param.Quantity);
                var product = new Product(Guid.NewGuid(), param.Name, param.Price, imageUrl, category, stock);

                await _productRepository.AddAsync(product);
                return product;
            });
        }
        catch
        {
            // 商品登録に失敗した場合、保存済みの画像を取り消す（孤児ファイルを残さない）
            if (imageUrl is not null)
            {
                await _imageStorage.DeleteAsync(imageUrl);
            }
            throw;
        }
    }
}