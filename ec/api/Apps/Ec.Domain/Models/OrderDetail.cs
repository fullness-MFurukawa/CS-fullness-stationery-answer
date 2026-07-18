using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 注文明細を表すドメインエンティティ
/// </summary>
/// <remarks>
/// orders_detailテーブルには単価のカラムがないため、単価は商品から取得する。
/// つまり商品の価格を変更すると、過去の注文明細の単価も変わることになる。
/// 本来、注文時点の単価は注文明細に記録すべきだが、与えられたテーブル定義に従う。
/// </remarks>
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
    /// コンストラクタ（データベースから復元する場合）
    /// </summary>
    /// <param name="id">注文明細ID</param>
    /// <param name="product">注文された商品</param>
    /// <param name="count">注文数</param>
    /// <exception cref="DomainException">商品が未指定、または注文数が1未満の場合</exception>
    public OrderDetail(int id, Product product, int count)
        : base(id)
    {
        Validate(product, count);
        Product = product;
        Count = count;
    }

    /// <summary>
    /// コンストラクタ（新規に生成する場合）
    /// </summary>
    /// <param name="product">注文された商品</param>
    /// <param name="count">注文数</param>
    /// <exception cref="DomainException">商品が未指定、または注文数が1未満の場合</exception>
    /// <remarks>
    /// 識別IDはデータベースの採番に委ねるため、この時点では設定しない。
    /// </remarks>
    public OrderDetail(Product product, int count)
        : base()
    {
        Validate(product, count);
        Product = product;
        Count = count;
    }

    /// <summary>
    /// 注文明細の制約を検証する
    /// </summary>
    /// <param name="product">注文された商品</param>
    /// <param name="count">注文数</param>
    /// <exception cref="DomainException">商品が未指定、または注文数が1未満の場合</exception>
    private static void Validate(Product product, int count)
    {
        if (product is null)
        {
            throw new DomainException("注文明細の商品が指定されていません。");
        }
        if (count < 1)
        {
            throw new DomainException("注文数は1以上で指定してください。");
        }
    }
}