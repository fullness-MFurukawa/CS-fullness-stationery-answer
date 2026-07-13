using Backend.Application.Interfaces;
using Backend.Application.Usecases;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC014:商品カテゴリ登録のユースケース実装
/// </summary>
public class CategoryRegisterInteractor : ICategoryRegisterUsecase
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productCategoryRepository">商品カテゴリのリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public CategoryRegisterInteractor(
        IProductCategoryRepository productCategoryRepository,
         IUnitOfWork unitOfWork)
    {
        _productCategoryRepository = productCategoryRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 新しい商品カテゴリを登録する
    /// </summary>
    /// <param name="name">商品カテゴリ名</param>
    /// <returns>登録された商品カテゴリ</returns>
    /// <exception cref="DomainException">商品カテゴリ名が未指定の場合</exception>
    public async Task<ProductCategory> ExecuteAsync(string name)
    {
        // 識別子はアプリケーション側で採番し、ドメインの不変条件で名前を検証する
        var category = new ProductCategory(Guid.NewGuid(), name);
        // トランザクション境界を制御して商品カテゴリを登録する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _productCategoryRepository.AddAsync(category);
            return category;
        });
    }
}