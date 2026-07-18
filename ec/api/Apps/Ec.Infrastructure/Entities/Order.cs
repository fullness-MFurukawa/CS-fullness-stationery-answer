using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ec.Infrastructure.Entities;

/// <summary>
/// 注文テーブル(orders)に対応するEF Coreエンティティ
/// </summary>
[Table("orders")]
public class Order
{
    /// <summary>
    /// 注文ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 注文識別ID
    /// </summary>
    [Column("order_uuid")]
    public Guid OrderUuid { get; set; }

    /// <summary>
    /// 注文日
    /// </summary>
    [Column("order_date")]
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// 合計金額
    /// </summary>
    [Column("amount_total")]
    public int AmountTotal { get; set; }

    /// <summary>
    /// 顧客ID(外部キー)
    /// </summary>
    [Column("customer_id")]
    public int CustomerId { get; set; }

    /// <summary>
    /// 注文ステータスID(外部キー)
    /// </summary>
    [Column("order_status_id")]
    public int OrderStatusId { get; set; }

    /// <summary>
    /// 支払い方法ID(外部キー)
    /// </summary>
    [Column("payment_method_id")]
    public int PaymentMethodId { get; set; }

    /// <summary>
    /// 注文した顧客（ナビゲーションプロパティ）
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// 注文ステータス（ナビゲーションプロパティ）
    /// </summary>
    public OrderStatus OrderStatus { get; set; } = null!;

    /// <summary>
    /// 支払い方法（ナビゲーションプロパティ）
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = null!;

    /// <summary>
    /// 注文明細の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}