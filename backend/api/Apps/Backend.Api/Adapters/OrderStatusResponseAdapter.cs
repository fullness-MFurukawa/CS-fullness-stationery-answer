using Backend.Api.ViewModels.Responses;
using Backend.Domain.Adapters;
using Backend.Domain.Models;

namespace Backend.Api.Adapters;

/// <summary>
/// 注文ステータスのドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
public class OrderStatusResponseAdapter : IEntityAdapter<OrderStatusResponse, OrderStatus>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する
    /// </summary>
    /// <param name="source">注文ステータスのレスポンス</param>
    /// <returns>ドメインの注文ステータス</returns>
    public OrderStatus ToDomain(OrderStatusResponse source)
        => new(source.OrderStatusId, source.Name);

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの注文ステータス</param>
    /// <returns>注文ステータスのレスポンス</returns>
    public OrderStatusResponse ToSource(OrderStatus domain)
        => new(domain.Id, domain.Name);
}