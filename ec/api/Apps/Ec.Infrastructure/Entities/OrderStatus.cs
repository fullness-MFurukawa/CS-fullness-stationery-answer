using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ec.Infrastructure.Entities;

/// <summary>
/// 注文ステータステーブル(order_status)に対応するEF Coreエンティティ
/// </summary>
[Table("order_status")]
public class OrderStatus
{
    /// <summary>
    /// 注文ステータスID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 注文ステータス名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// このステータスを持つ注文の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}