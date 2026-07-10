using Backend.Domain.Adapters;

using DomainProductCategory = Backend.Domain.Models.ProductCategory;
using EfProductCategory = Backend.Infrastructure.Entities.ProductCategory;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 商品カテゴリのEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class ProductCategoryAdapter : IEntityAdapter<EfProductCategory, DomainProductCategory>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainProductCategory ToDomain(EfProductCategory source)
        => new(source.CategoryUuid, source.Name);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfProductCategory ToSource(DomainProductCategory domain)
        => new()
        {
            CategoryUuid = domain.Id,
            Name = domain.Name
        };
}