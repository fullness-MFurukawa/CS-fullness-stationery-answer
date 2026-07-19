using Ec.Api.ViewModels.Responses;
using Ec.Domain.Adapters;
using Ec.Domain.Models;
namespace Ec.Api.Adapters;

/// <summary>
/// 注文のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>復元しないため逆方向は未サポートとする。</remarks>
public class OrderResponseAdapter : IEntityAdapter<OrderResponse, Order>
{
    private readonly OrderDetailResponseAdapter _detailAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="detailAdapter">注文明細のレスポンスアダプタ</param>
    public OrderResponseAdapter(OrderDetailResponseAdapter detailAdapter)
    {
        _detailAdapter = detailAdapter;
    }

    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">注文のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public Order ToDomain(OrderResponse source)
        => throw new NotSupportedException("レスポンスから注文への復元は行えません。");

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
            domain.Status.Name,
            domain.PaymentMethod.Name,
            domain.Details.Select(_detailAdapter.ToSource).ToList());
}