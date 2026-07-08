using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;
/// <summary>
/// 商品在庫を表すドメインエンティティ
/// </summary>
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
}