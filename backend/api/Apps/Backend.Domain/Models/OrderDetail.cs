using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;

/// <summary>
/// 注文明細を表すドメインエンティティ
/// </summary>
public sealed class OrderDetail : Entity<int>
{
    /// <summary>
    /// 注文された商品
    /// </summary>
    public Product Product { get; }

    /// <summary>
    /// 注文数
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// 小計（単価 × 注文数）
    /// </summary>
    public int Subtotal => Product.Price * Count;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">注文明細ID</param>
    /// <param name="product">注文された商品</param>
    /// <param name="count">注文数</param>
    /// <exception cref="DomainException">商品が未指定、または注文数が1未満の場合</exception>
    public OrderDetail(int id, Product product, int count)
        : base(id)
    {
        if (product is null)
        {
            throw new DomainException("注文明細の商品が指定されていません。");
        }
        if (count < 1)
        {
            throw new DomainException("注文数は1以上で指定してください。");
        }
        Product = product;
        Count = count;
    }
}