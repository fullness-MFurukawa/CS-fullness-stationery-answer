using Backend.Domain.Adapters;

using DomainProductStock = Backend.Domain.Models.ProductStock;
using EfProductStock = Backend.Infrastructure.Entities.ProductStock;

namespace Backend.Infrastructure.Adapters;

/// <summary>
/// 商品在庫のEFエンティティとドメインエンティティを相互変換するアダプタ
/// </summary>
public class ProductStockAdapter : IEntityAdapter<EfProductStock, DomainProductStock>
{
    /// <summary>
    /// EFエンティティからドメインエンティティへ変換
    /// </summary>
    /// <param name="source">変換元のEFエンティティ</param>
    /// <returns>ドメインエンティティ</returns>
    public DomainProductStock ToDomain(EfProductStock source)
        => new(source.StockUuid, source.Quantity);

    /// <summary>
    /// ドメインエンティティからEFエンティティへ変換
    /// </summary>
    /// <param name="domain">変換元のドメインエンティティ</param>
    /// <returns>EFエンティティ</returns>
    public EfProductStock ToSource(DomainProductStock domain)
        => new()
        {
            StockUuid = domain.Id,
            Quantity = domain.Quantity
        };
}