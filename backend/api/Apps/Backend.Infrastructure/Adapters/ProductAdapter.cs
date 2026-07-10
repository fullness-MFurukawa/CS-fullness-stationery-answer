using DomainProduct = Backend.Domain.Models.Product;
using DomainProductCategory = Backend.Domain.Models.ProductCategory;
using DomainProductStock = Backend.Domain.Models.ProductStock;
using EfProduct = Backend.Infrastructure.Entities.Product;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 商品(Product)とEFエンティティ(ProductEntity)を相互変換するアダプタ
/// 関連（カテゴリ・在庫）の変換は行わず、変換済みのものを受け取る
/// </summary>
public class ProductAdapter
{
    /// <summary>
    /// EFエンティティと変換済みの関連からドメインの商品を生成
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <param name="category">変換済みの商品カテゴリ</param>
    /// <param name="stock">変換済みの商品在庫</param>
    /// <returns>ドメインの商品</returns>
    public DomainProduct ToDomain(EfProduct source, DomainProductCategory category, DomainProductStock stock)
        => new(
            source.ProductUuid,
            source.Name,
            source.Price,
            source.ImageUrl,
            category,
            stock,
            source.DeleteFlg != 0);

    /// <summary>
    /// ドメインの商品からEFエンティティへ変換（スカラー項目のみ）
    /// </summary>
    /// <param name="domain">変換元のドメインの商品</param>
    /// <returns>EFエンティティ</returns>
    public EfProduct ToSource(DomainProduct domain)
        => new()
        {
            ProductUuid = domain.Id,
            Name = domain.Name,
            Price = domain.Price,
            ImageUrl = domain.ImageUrl,
            DeleteFlg = domain.IsDeleted ? 1 : 0
        };
}