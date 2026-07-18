using Ec.Domain.Adapters;

using DomainPaymentMethod = Ec.Domain.Models.PaymentMethod;
using EfPaymentMethod = Ec.Infrastructure.Entities.PaymentMethod;

namespace Ec.Infrastructure.Adapters;

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