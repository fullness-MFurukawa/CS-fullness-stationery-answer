using Backend.Domain.Adapters;

using DomainPaymentMethod = Backend.Domain.Models.PaymentMethod;
using EfPaymentMethod = Backend.Infrastructure.Entities.PaymentMethod;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 支払い方法のEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class PaymentMethodAdapter : IEntityAdapter<EfPaymentMethod, DomainPaymentMethod>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainPaymentMethod ToDomain(EfPaymentMethod source)
        => new(source.Id, source.Name);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfPaymentMethod ToSource(DomainPaymentMethod domain)
        => new()
        {
            Id = domain.Id,
            Name = domain.Name
        };
}