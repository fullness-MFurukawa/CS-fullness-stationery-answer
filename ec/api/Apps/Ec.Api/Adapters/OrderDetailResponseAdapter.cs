using Ec.Api.ViewModels.Responses;
using Ec.Domain.Adapters;
using Ec.Domain.Models;
namespace Ec.Api.Adapters;

/// <summary>
/// 注文明細のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>復元しないため逆方向は未サポートとする。</remarks>
public class OrderDetailResponseAdapter : IEntityAdapter<OrderDetailResponse, OrderDetail>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">注文明細のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public OrderDetail ToDomain(OrderDetailResponse source)
        => throw new NotSupportedException("レスポンスから注文明細への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの注文明細</param>
    /// <returns>注文明細のレスポンス</returns>
    public OrderDetailResponse ToSource(OrderDetail domain)
        => new(
            domain.Product.Id,
            domain.Product.Name,
            domain.Product.Price,
            domain.Count,
            domain.Subtotal);
}