using DomainOrderDetail = Ec.Domain.Models.OrderDetail;
using DomainProduct = Ec.Domain.Models.Product;
using EfOrderDetail = Ec.Infrastructure.Entities.OrderDetail;

namespace Ec.Infrastructure.Adapters;

/// <summary>
/// 注文明細(OrderDetail)とEFエンティティを相互変換するアダプタ
/// 関連（商品）の変換は行わず、変換済みのものを受け取る
/// </summary>
public class OrderDetailAdapter
{
    /// <summary>
    /// EFエンティティと変換済みの商品からドメインの注文明細を生成
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <param name="product">変換済みの商品</param>
    /// <returns>ドメインの注文明細</returns>
    public DomainOrderDetail ToDomain(EfOrderDetail source, DomainProduct product)
        => new(source.Id, product, source.Count);

    /// <summary>
    /// ドメインの注文明細からEFエンティティへ変換（スカラー項目のみ）
    /// </summary>
    /// <param name="domain">変換元のドメインの注文明細</param>
    /// <returns>EFエンティティ</returns>
    public EfOrderDetail ToSource(DomainOrderDetail domain)
        => new()
        {
            Id = domain.Id,
            Count = domain.Count
        };
}