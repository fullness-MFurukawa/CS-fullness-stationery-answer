using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 商品カテゴリテーブル(product_category)に対応するEF Coreエンティティ
/// </summary>
[Table("product_category")]
public class ProductCategory
{
    /// <summary>
    /// 商品カテゴリID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 商品カテゴリ識別ID
    /// </summary>
    [Column("category_uuid")]
    public Guid CategoryUuid { get; set; }

    /// <summary>
    /// 商品カテゴリ名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// このカテゴリに属する商品の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}