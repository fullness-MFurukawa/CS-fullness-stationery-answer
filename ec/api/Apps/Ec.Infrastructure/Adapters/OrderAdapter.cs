using DomainCustomer = Ec.Domain.Models.Customer;
using DomainOrder = Ec.Domain.Models.Order;
using DomainOrderDetail = Ec.Domain.Models.OrderDetail;
using DomainOrderStatus = Ec.Domain.Models.OrderStatus;
using DomainPaymentMethod = Ec.Domain.Models.PaymentMethod;
using EfOrder = Ec.Infrastructure.Entities.Order;

namespace Ec.Infrastructure.Adapters;

/// <summary>
/// 注文(Order)とEFエンティティを相互変換するアダプタ
/// 関連（顧客・ステータス・支払い方法・明細）の変換は行わず、変換済みのものを受け取る
/// </summary>
public class OrderAdapter
{
    /// <summary>
    /// EFエンティティと変換済みの関連からドメインの注文を生成
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <param name="customer">変換済みの顧客</param>
    /// <param name="status">変換済みの注文ステータス</param>
    /// <param name="paymentMethod">変換済みの支払い方法</param>
    /// <param name="details">変換済みの注文明細の一覧</param>
    /// <returns>ドメインの注文</returns>
    public DomainOrder ToDomain(
        EfOrder source,
        DomainCustomer customer,
        DomainOrderStatus status,
        DomainPaymentMethod paymentMethod,
        IReadOnlyList<DomainOrderDetail> details)
        => new(
            source.OrderUuid,
            source.OrderDate,
            source.AmountTotal,
            customer,
            status,
            paymentMethod,
            details);

    /// <summary>
    /// ドメインの注文からEFエンティティへ変換（スカラー項目のみ）
    /// </summary>
    /// <param name="domain">変換元のドメインの注文</param>
    /// <returns>EFエンティティ</returns>
    public EfOrder ToSource(DomainOrder domain)
        => new()
        {
            OrderUuid = domain.Id,
            OrderDate = domain.OrderDate,
            AmountTotal = domain.AmountTotal
        };
}