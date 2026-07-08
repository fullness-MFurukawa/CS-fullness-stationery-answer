using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;
/// <summary>
/// 商品を表すドメインエンティティ（商品集約のルート）
/// </summary>
public sealed class Product : Entity<Guid>
{
    /// <summary>
    /// 商品名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 価格
    /// </summary>
    public int Price { get; }

    /// <summary>
    /// 画像URL
    /// </summary>
    public string? ImageUrl { get; }

    /// <summary>
    /// 商品カテゴリ
    /// </summary>
    public ProductCategory Category { get; }

    /// <summary>
    /// 商品在庫
    /// </summary>
    public ProductStock Stock { get; }

    /// <summary>
    /// 論理削除済みかどうか
    /// </summary>
    public bool IsDeleted { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">商品識別ID(uuid)</param>
    /// <param name="name">商品名</param>
    /// <param name="price">価格</param>
    /// <param name="imageUrl">画像URL</param>
    /// <param name="category">商品カテゴリ</param>
    /// <param name="stock">商品在庫</param>
    /// <param name="isDeleted">論理削除済みかどうか</param>
    /// <exception cref="DomainException">商品名が未指定、価格が負数、商品カテゴリが未指定、または商品在庫が未指定の場合</exception>
    public Product(
        Guid id,
        string name,
        int price,
        string? imageUrl,
        ProductCategory category,
        ProductStock stock,
        bool isDeleted = false)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("商品名が指定されていません。");
        }
        if (price < 0)
        {
            throw new DomainException("価格に負の値は指定できません。");
        }
        if (category is null)
        {
            throw new DomainException("商品カテゴリが指定されていません。");
        }
        if (stock is null)
        {
            throw new DomainException("商品在庫が指定されていません。");
        }
        Name = name;
        Price = price;
        ImageUrl = imageUrl;
        Category = category;
        Stock = stock;
        IsDeleted = isDeleted;
    }
}