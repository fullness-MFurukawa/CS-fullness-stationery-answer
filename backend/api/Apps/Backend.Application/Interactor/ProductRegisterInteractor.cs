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

    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public ProductRegisterInteractor(
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 新しい商品を在庫とともに登録する
    /// </summary>
    /// <param name="param">新商品登録の入力値</param>
    /// <returns>登録された商品</returns>
    /// <exception cref="NotFoundException">指定された商品カテゴリが存在しない場合</exception>
    /// <exception cref="DomainException">商品名・価格・在庫数が不正な場合</exception>
    public async Task<Product> ExecuteAsync(ProductRegisterParam param)
    {
        // トランザクション境界を制御して商品を登録する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 商品カテゴリを取得する（商品集約の構築に必要）
            var category = await _productCategoryRepository.FindByIdAsync(param.CategoryId)
                ?? throw new NotFoundException("指定された商品カテゴリが存在しません。");

            // 在庫を含む商品集約を構築する（入力値の妥当性はドメインの不変条件が担保する）
            var stock = new ProductStock(Guid.NewGuid(), param.Quantity);
            var product = new Product(Guid.NewGuid(), param.Name, param.Price, param.ImageUrl, category, stock);

            await _productRepository.AddAsync(product);

            return product;
        });
    }
}