using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

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
    /// 画像が未設定の場合はnull
    /// </summary>
    /// <remarks>
    /// 空文字とnullが混在すると「画像なし」の表現が2通りになるため、
    /// 空文字および空白のみの文字列はnullへ正規化する。
    /// </remarks>
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
    /// <remarks>
    /// EC側では論理削除された商品を表示しないため、
    /// 通常この値がtrueの商品を取得することはない。
    /// ただし、カートに入れた後に管理者が削除した場合を考慮し、
    /// 購入時の判定に用いる。
    /// </remarks>
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
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl;
        Category = category;
        Stock = stock;
        IsDeleted = isDeleted;
    }

    /// <summary>
    /// 指定した数量を購入できるかどうかを判定する（UC004）
    /// </summary>
    /// <param name="count">購入する数量</param>
    /// <returns>購入できる場合はtrue</returns>
    public bool CanPurchase(int count) => !IsDeleted && Stock.CanPurchase(count);

    /// <summary>
    /// 購入した数量分、在庫を減らす（UC005）
    /// </summary>
    /// <param name="count">購入する数量</param>
    /// <returns>在庫を減らした後の商品</returns>
    /// <exception cref="DomainException">販売終了した商品の場合、数量が0以下の場合、または在庫数を上回る場合</exception>
    /// <remarks>
    /// 在庫の増減は集約のルートである商品を経由して行う。
    /// 在庫だけを直接操作できてしまうと、
    /// 「販売終了した商品の在庫が減る」といった不整合を防げなくなる。
    /// </remarks>
    public Product ReduceStock(int count)
    {
        if (IsDeleted)
        {
            throw new DomainException($"「{Name}」は販売を終了しました。");
        }
        return new Product(Id, Name, Price, ImageUrl, Category, Stock.Reduce(count), IsDeleted);
    }
}