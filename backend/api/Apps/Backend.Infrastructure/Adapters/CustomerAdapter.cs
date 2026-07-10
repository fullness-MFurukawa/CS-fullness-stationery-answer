using Backend.Domain.Adapters;

using DomainCustomer = Backend.Domain.Models.Customer;
using EfCustomer = Backend.Infrastructure.Entities.Customer;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 顧客のEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class CustomerAdapter : IEntityAdapter<EfCustomer, DomainCustomer>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainCustomer ToDomain(EfCustomer source)
        => new(
            source.CustomerUuid,
            source.Name,
            source.NameKana,
            source.Address1,
            source.Address2,
            source.PhoneNumber,
            source.MailAddress,
            source.Username,
            source.Password,
            source.CreatedAt);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfCustomer ToSource(DomainCustomer domain)
        => new()
        {
            CustomerUuid = domain.Id,
            Name = domain.Name,
            NameKana = domain.NameKana,
            Address1 = domain.Address1,
            Address2 = domain.Address2,
            PhoneNumber = domain.PhoneNumber,
            MailAddress = domain.MailAddress,
            Username = domain.Username,
            Password = domain.Password,
            CreatedAt = domain.CreatedAt
        };
}