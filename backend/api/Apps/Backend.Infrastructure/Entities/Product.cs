using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 商品テーブル(product)に対応するEF Coreエンティティ
/// </summary>
[Table("product")]
public class Product
{
    /// <summary>
    /// 商品ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 商品識別ID
    /// </summary>
    [Column("product_uuid")]
    public Guid ProductUuid { get; set; }

    /// <summary>
    /// 商品名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 価格
    /// </summary>
    [Column("price")]
    public int Price { get; set; }

    /// <summary>
    /// 画像URL
    /// </summary>
    [Column("image_url")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// 商品カテゴリID(外部キー)
    /// </summary>
    [Column("product_category_id")]
    public int ProductCategoryId { get; set; }

    /// <summary>
    /// 削除フラグ(0:通常, 1:削除)
    /// </summary>
    [Column("delete_flg")]
    public int DeleteFlg { get; set; }

    /// <summary>
    /// 商品カテゴリ（ナビゲーションプロパティ）
    /// </summary>
    public ProductCategory Category { get; set; } = null!;

    /// <summary>
    /// 商品在庫（ナビゲーションプロパティ、1対1）
    /// </summary>
    public ProductStock? Stock { get; set; }

    /// <summary>
    /// この商品を含む注文明細の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}