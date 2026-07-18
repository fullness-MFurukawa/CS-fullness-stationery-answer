using Ec.Domain.Exceptions;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Factories;

using EfProduct = Ec.Infrastructure.Entities.Product;
using EfProductCategory = Ec.Infrastructure.Entities.ProductCategory;
using EfProductStock = Ec.Infrastructure.Entities.ProductStock;

namespace Ec.Infrastructure.Tests.Factories;

[TestClass]
[TestCategory("Backend.Infrastructure.Factories")]
public class ProductFactoryTests
{
    private readonly ProductFactory _factory = new(
        new ProductCategoryAdapter(),
        new ProductStockAdapter(),
        new ProductAdapter());

    /// <summary>
    /// テスト用のEF商品エンティティを生成する
    /// </summary>
    /// <param name="deleteFlg">削除フラグ</param>
    /// <param name="withCategory">カテゴリをロード済みにするか</param>
    /// <param name="withStock">在庫をロード済みにするか</param>
    private static EfProduct CreateEntity(
        int deleteFlg = 0,
        bool withCategory = true,
        bool withStock = true)
    {
        var entity = new EfProduct
        {
            Id = 1,
            ProductUuid = Guid.NewGuid(),
            Name = "水性ボールペン(黒)",
            Price = 120,
            ImageUrl = null,
            ProductCategoryId = 1,
            DeleteFlg = deleteFlg
        };

        if (withCategory)
        {
            entity.Category = new EfProductCategory
            {
                Id = 1,
                CategoryUuid = Guid.NewGuid(),
                Name = "文房具"
            };
        }

        if (withStock)
        {
            entity.Stock = new EfProductStock
            {
                Id = 1,
                StockUuid = Guid.NewGuid(),
                Quantity = 10,
                ProductId = 1
            };
        }

        return entity;
    }

    [TestMethod(DisplayName = "関連がロード済みなら商品集約を組み立てられる")]
    public void Create_WithLoadedRelations_ReturnsProductAggregate()
    {
        var entity = CreateEntity();

        var product = _factory.Create(entity);

        Assert.AreEqual(entity.ProductUuid, product.Id);
        Assert.AreEqual("水性ボールペン(黒)", product.Name);
        Assert.AreEqual(120, product.Price);
        Assert.IsNull(product.ImageUrl);
        Assert.IsFalse(product.IsDeleted);
    }

    [TestMethod(DisplayName = "組み立てた商品集約にカテゴリと在庫が含まれる")]
    public void Create_WithLoadedRelations_IncludesCategoryAndStock()
    {
        var entity = CreateEntity();

        var product = _factory.Create(entity);

        Assert.AreEqual(entity.Category.CategoryUuid, product.Category.Id);
        Assert.AreEqual("文房具", product.Category.Name);
        Assert.AreEqual(entity.Stock!.StockUuid, product.Stock.Id);
        Assert.AreEqual(10, product.Stock.Quantity);
    }

    [TestMethod(DisplayName = "削除フラグが1なら論理削除済みとして組み立てられる")]
    public void Create_DeleteFlgIsOne_IsDeletedIsTrue()
    {
        var entity = CreateEntity(deleteFlg: 1);

        var product = _factory.Create(entity);

        Assert.IsTrue(product.IsDeleted);
    }

    [TestMethod(DisplayName = "カテゴリが未ロードならDomainExceptionをスローする")]
    public void Create_CategoryNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withCategory: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "在庫が未ロードならDomainExceptionをスローする")]
    public void Create_StockNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withStock: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }
}