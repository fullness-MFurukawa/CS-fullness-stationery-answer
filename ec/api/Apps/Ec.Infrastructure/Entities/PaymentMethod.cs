using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ec.Infrastructure.Entities;

/// <summary>
/// 支払い方法テーブル(payment_method)に対応するEF Coreエンティティ
/// </summary>
[Table("payment_method")]
public class PaymentMethod
{
    /// <summary>
    /// 支払い方法ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 支払い方法名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// この支払い方法を持つ注文の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}