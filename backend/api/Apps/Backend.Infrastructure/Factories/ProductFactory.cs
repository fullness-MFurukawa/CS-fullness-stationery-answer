using Backend.Domain.Adapters;
using Backend.Domain.Exceptions;
using Backend.Infrastructure.Adapters;

using DomainProduct = Backend.Domain.Models.Product;
using EfProduct = Backend.Infrastructure.Entities.Product;

namespace Backend.Infrastructure.Factories;

/// <summary>
/// EFの商品エンティティからドメインの商品集約を組み立てるファクトリ
/// </summary>
public class ProductFactory : IAggregateFactory<EfProduct, DomainProduct>
{
    private readonly ProductCategoryAdapter _categoryAdapter;
    private readonly ProductStockAdapter _stockAdapter;
    private readonly ProductAdapter _productAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryAdapter">商品カテゴリのアダプタ</param>
    /// <param name="stockAdapter">商品在庫のアダプタ</param>
    /// <param name="productAdapter">商品のアダプタ</param>
    public ProductFactory(
        ProductCategoryAdapter categoryAdapter,
        ProductStockAdapter stockAdapter,
        ProductAdapter productAdapter)
    {
        _categoryAdapter = categoryAdapter;
        _stockAdapter = stockAdapter;
        _productAdapter = productAdapter;
    }

    /// <summary>
    /// EFの商品エンティティ（Category・Stock を Include 済み）から商品集約を組み立てる
    /// </summary>
    /// <param name="source">組み立て元のEFエンティティ</param>
    /// <returns>商品集約（ドメイン）</returns>
    public DomainProduct Create(EfProduct source)
    {
        // 関連が未ロードなら組み立て不可（Includeの指定漏れ）
        if (source.Category is null)
        {
            throw new DomainException("商品カテゴリが読み込まれていません。");
        }
        if (source.Stock is null)
        {
            throw new DomainException("商品在庫が読み込まれていません。");
        }

        // 関連（カテゴリ・在庫）を各Adapterで変換
        var category = _categoryAdapter.ToDomain(source.Category);
        var stock = _stockAdapter.ToDomain(source.Stock!);

        // 変換結果を使って商品集約を組み立てる
        return _productAdapter.ToDomain(source, category, stock);
    }
}