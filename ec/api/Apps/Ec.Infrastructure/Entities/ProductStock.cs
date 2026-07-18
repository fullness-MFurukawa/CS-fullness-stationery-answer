using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ec.Infrastructure.Entities;

/// <summary>
/// 商品在庫テーブル(product_stock)に対応するEF Coreエンティティ
/// </summary>
[Table("product_stock")]
public class ProductStock
{
    /// <summary>
    /// 商品在庫ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 商品在庫識別ID
    /// </summary>
    [Column("stock_uuid")]
    public Guid StockUuid { get; set; }

    /// <summary>
    /// 商品在庫数
    /// </summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>
    /// 商品ID(外部キー)
    /// </summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// 対象の商品（ナビゲーションプロパティ、1対1）
    /// </summary>
    public Product Product { get; set; } = null!;
}