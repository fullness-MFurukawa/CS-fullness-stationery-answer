using Ec.Api.ViewModels.Responses;
using Ec.Domain.Adapters;
using Ec.Domain.Models;
namespace Ec.Api.Adapters;

/// <summary>
/// 商品カテゴリのドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは表示に必要な項目のみを持ち復元しないため、逆方向は未サポートとする。
/// </remarks>
public class CategoryResponseAdapter : IEntityAdapter<CategoryResponse, ProductCategory>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">商品カテゴリのレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public ProductCategory ToDomain(CategoryResponse source)
        => throw new NotSupportedException("レスポンスから商品カテゴリへの復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの商品カテゴリ</param>
    /// <returns>商品カテゴリのレスポンス</returns>
    public CategoryResponse ToSource(ProductCategory domain)
        => new(domain.Id, domain.Name);
}