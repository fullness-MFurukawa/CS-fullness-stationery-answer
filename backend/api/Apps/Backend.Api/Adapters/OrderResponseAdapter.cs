using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 注文のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは注文集約を平坦化したものであり、顧客・支払い方法・商品の識別情報を持たないため復元できない。
/// </remarks>
public class OrderResponseAdapter : IEntityAdapter<OrderResponse, Order>
{
    private readonly OrderDetailResponseAdapter _orderDetailResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderDetailResponseAdapter">注文明細のアダプタ</param>
    public OrderResponseAdapter(OrderDetailResponseAdapter orderDetailResponseAdapter)
    {
        _orderDetailResponseAdapter = orderDetailResponseAdapter;
    }

    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">注文のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">注文集約を復元できないため</exception>
    public Order ToDomain(OrderResponse source)
        => throw new NotSupportedException("レスポンスから注文集約への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの注文</param>
    /// <returns>注文のレスポンス</returns>
    public OrderResponse ToSource(Order domain)
        => new(
            domain.Id,
            domain.OrderDate,
            domain.AmountTotal,
            domain.Customer.Username,
            domain.Customer.Name,
            domain.Status.Id,
            domain.Status.Name,
            domain.PaymentMethod.Name,
            domain.Details.Select(_orderDetailResponseAdapter.ToSource).ToList());
}