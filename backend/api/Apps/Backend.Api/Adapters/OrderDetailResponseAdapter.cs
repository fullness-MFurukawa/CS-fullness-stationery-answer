using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 注文明細のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは商品名と単価のみを持ち、商品集約を含まないため復元できない。
/// </remarks>
public class OrderDetailResponseAdapter : IEntityAdapter<OrderDetailResponse, OrderDetail>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">注文明細のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">商品集約を復元できないため</exception>
    public OrderDetail ToDomain(OrderDetailResponse source)
        => throw new NotSupportedException("レスポンスから注文明細への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの注文明細</param>
    /// <returns>注文明細のレスポンス</returns>
    public OrderDetailResponse ToSource(OrderDetail domain)
        => new(
            domain.Product.Name,
            domain.Product.Price,
            domain.Count,
            domain.Subtotal);
}