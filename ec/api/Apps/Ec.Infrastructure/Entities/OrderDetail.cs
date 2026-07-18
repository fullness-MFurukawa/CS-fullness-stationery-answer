using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ec.Infrastructure.Entities;

/// <summary>
/// 注文明細テーブル(orders_detail)に対応するEF Coreエンティティ
/// </summary>
[Table("orders_detail")]
public class OrderDetail
{
    /// <summary>
    /// 注文明細ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 注文ID(外部キー)
    /// </summary>
    [Column("order_id")]
    public int OrderId { get; set; }

    /// <summary>
    /// 商品ID(外部キー)
    /// </summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// 注文数
    /// </summary>
    [Column("count")]
    public int Count { get; set; }

    /// <summary>
    /// 対象の注文（ナビゲーションプロパティ）
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// 対象の商品（ナビゲーションプロパティ）
    /// </summary>
    public Product Product { get; set; } = null!;
}