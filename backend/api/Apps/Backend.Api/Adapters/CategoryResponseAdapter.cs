using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 商品カテゴリのドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
public class CategoryResponseAdapter : IEntityAdapter<CategoryResponse, ProductCategory>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する
    /// </summary>
    /// <param name="source">商品カテゴリのレスポンス</param>
    /// <returns>ドメインの商品カテゴリ</returns>
    public ProductCategory ToDomain(CategoryResponse source)
        => new(source.CategoryId, source.Name);

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの商品カテゴリ</param>
    /// <returns>商品カテゴリのレスポンス</returns>
    public CategoryResponse ToSource(ProductCategory domain)
        => new(domain.Id, domain.Name);
}