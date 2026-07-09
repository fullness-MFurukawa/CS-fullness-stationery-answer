using Backend.Domain.Adapters;
using EfOrderStatus = Backend.Infrastructure.Entities.OrderStatus;
using DomainOrderStatus = Backend.Domain.Models.OrderStatus;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 注文ステータスのEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class OrderStatusAdapter : IEntityAdapter<EfOrderStatus, DomainOrderStatus>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainOrderStatus ToDomain(EfOrderStatus source)
        => new(source.Id, source.Name);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfOrderStatus ToSource(DomainOrderStatus domain)
        => new()
        {
            Id = domain.Id,
            Name = domain.Name
        };
}