using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 商品のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは商品集約を平坦化したものであり、在庫の識別IDなど一部の情報を持たない。
/// そのためレスポンスからドメインオブジェクトへの復元は行えず、未サポートとする。
/// </remarks>
public class ProductResponseAdapter : IEntityAdapter<ProductResponse, Product>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">商品のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">在庫の識別IDが失われるため復元できない</exception>
    public Product ToDomain(ProductResponse source)
        => throw new NotSupportedException("レスポンスから商品集約への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの商品</param>
    /// <returns>商品のレスポンス</returns>
    public ProductResponse ToSource(Product domain)
        => new(
            domain.Id,
            domain.Name,
            domain.Price,
            domain.ImageUrl,
            domain.Stock.Quantity,
            domain.Category.Id,
            domain.Category.Name,
            domain.IsDeleted);
}