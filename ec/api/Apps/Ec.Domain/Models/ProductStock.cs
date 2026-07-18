using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 商品在庫を表すドメインエンティティ
/// </summary>
/// <remarks>
/// 管理サービス側の在庫は「担当者が設定する値」であったが、
/// EC側の在庫は「購入によって減る値」である。
/// 在庫を割り込ませないという業務ルールを、このエンティティが担う。
/// </remarks>
public sealed class ProductStock : Entity<Guid>
{
    /// <summary>
    /// 在庫数
    /// </summary>
    public int Quantity { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">商品在庫識別ID(uuid)</param>
    /// <param name="quantity">在庫数</param>
    /// <exception cref="DomainException">在庫数が負数の場合</exception>
    public ProductStock(Guid id, int quantity) : base(id)
    {
        if (quantity < 0)
        {
            throw new DomainException("在庫数に負の値は指定できません。");
        }
        Quantity = quantity;
    }

    /// <summary>
    /// 指定した数量を購入できるかどうかを判定する
    /// </summary>
    /// <param name="count">購入する数量</param>
    /// <returns>購入できる場合はtrue</returns>
    public bool CanPurchase(int count) => count > 0 && Quantity >= count;

    /// <summary>
    /// 購入した数量分、在庫を減らす（UC005）
    /// </summary>
    /// <param name="count">購入する数量</param>
    /// <returns>在庫を減らした後の商品在庫</returns>
    /// <exception cref="DomainException">数量が0以下の場合、または在庫数を上回る場合</exception>
    /// <remarks>
    /// 自身を書き換えるのではなく、新しいインスタンスを返す。
    /// エンティティを不変に保つと、処理の途中で状態が変わることがなくなり、
    /// 「どの時点の在庫か」が追いやすくなる。
    /// </remarks>
    public ProductStock Reduce(int count)
    {
        if (count <= 0)
        {
            throw new DomainException("購入数量には1以上を指定してください。");
        }
        if (Quantity < count)
        {
            throw new DomainException($"在庫が不足しています。（在庫数: {Quantity}、購入数量: {count}）");
        }
        return new ProductStock(Id, Quantity - count);
    }
}