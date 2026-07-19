using Ec.Domain.Models;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Factories;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class ProductRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private ProductRepository CreateRepository()
        => new(
            Context,
            new ProductStockAdapter(),
            new ProductFactory(new ProductCategoryAdapter(), new ProductStockAdapter(), new ProductAdapter()));

    /// <summary>
    /// 名前を指定して商品カテゴリを取得する
    /// </summary>
    private async Task<ProductCategory> GetCategoryAsync(string name)
    {
        var categoryRepository = new ProductCategoryRepository(Context, new ProductCategoryAdapter());
        return (await categoryRepository.FindAllAsync()).First(c => c.Name == name);
    }

    [TestMethod(DisplayName = "論理削除を除いた有効な商品をすべて取得できる")]
    public async Task FindAllAsync_ReturnsOnlyActiveProducts()
    {
        var repository = CreateRepository();

        var products = await repository.FindAllAsync();

        Assert.HasCount(13, products);
        Assert.IsFalse(products.Any(p => p.IsDeleted));
        Assert.IsFalse(products.Any(p => p.Name == "廃番ボールペン"));
    }

    [TestMethod(DisplayName = "取得した商品にカテゴリと在庫が含まれる")]
    public async Task FindAllAsync_IncludesCategoryAndStock()
    {
        var repository = CreateRepository();

        var products = await repository.FindAllAsync();

        var first = products[0];
        Assert.AreEqual("水性ボールペン(黒)", first.Name);
        Assert.AreEqual("文房具", first.Category.Name);
        // 在庫数は購入で変わるため、具体値ではなく取得できていることを確認する
        Assert.IsGreaterThanOrEqualTo(0, first.Stock.Quantity);
    }

    [TestMethod(DisplayName = "指定カテゴリの有効な商品のみ取得できる")]
    public async Task FindByCategoryAsync_ReturnsActiveProductsOfCategory()
    {
        var repository = CreateRepository();
        var category = await GetCategoryAsync("文房具");

        var products = await repository.FindByCategoryAsync(category.Id);

        Assert.HasCount(5, products);
        Assert.IsTrue(products.All(p => p.Category.Name == "文房具"));
        Assert.IsFalse(products.Any(p => p.Name == "廃番ボールペン"));
    }

    [TestMethod(DisplayName = "識別IDを指定して有効な商品を取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsProduct()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual("水性ボールペン(黒)", actual.Name);
        Assert.AreEqual("文房具", actual.Category.Name);
        // 在庫数は購入で変わるため、具体値では比較しない
        Assert.IsGreaterThanOrEqualTo(0, actual.Stock.Quantity);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }

    [TestMethod(DisplayName = "論理削除された商品はFindByIdAsyncでnullを返す")]
    public async Task FindByIdAsync_DeletedProduct_ReturnsNull()
    {
        var repository = CreateRepository();
        // 削除済みの「廃番ボールペン」のUUIDを、EF経由で直接引く。
        // 管理側と異なり、EC側のFindByIdAsyncは論理削除を除外する
        var deletedUuid = Context.Products.First(p => p.Name == "廃番ボールペン").ProductUuid;

        var actual = await repository.FindByIdAsync(deletedUuid);

        Assert.IsNull(actual);
    }

    [TestMethod(DisplayName = "指定した商品を悲観的ロック付きで取得できる")]
    public async Task FindByIdsForUpdateAsync_ReturnsProducts()
    {
        var repository = CreateRepository();
        var all = await repository.FindAllAsync();
        var ids = new[] { all[0].Id, all[1].Id };

        var products = await repository.FindByIdsForUpdateAsync(ids);

        Assert.HasCount(2, products);
        Assert.IsTrue(products.Any(p => p.Id == all[0].Id));
        Assert.IsTrue(products.Any(p => p.Id == all[1].Id));
        // カテゴリと在庫も組み立てられている
        Assert.IsTrue(products.All(p => p.Stock.Quantity >= 0));
        Assert.IsTrue(products.All(p => !string.IsNullOrEmpty(p.Category.Name)));
    }

    [TestMethod(DisplayName = "論理削除された商品はロック取得の対象に含まれない")]
    public async Task FindByIdsForUpdateAsync_ExcludesDeletedProduct()
    {
        var repository = CreateRepository();
        var deletedUuid = Context.Products.First(p => p.Name == "廃番ボールペン").ProductUuid;

        var products = await repository.FindByIdsForUpdateAsync([deletedUuid]);

        // 削除済みは除外されるため0件。
        // 購入確定時、カート投入後に削除された商品を弾く根拠になる
        Assert.HasCount(0, products);
    }

    [TestMethod(DisplayName = "在庫数を更新できる")]
    public async Task UpdateStockAsync_UpdatesQuantity()
    {
        var repository = CreateRepository();
        var product = (await repository.FindAllAsync())[0];

        // 在庫を減らした商品を作り、更新する
        var reduced = product.ReduceStock(3);
        await repository.UpdateStockAsync(reduced);

        var saved = await repository.FindByIdAsync(product.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual(product.Stock.Quantity - 3, saved!.Stock.Quantity);
        // 在庫以外は変わらない
        Assert.AreEqual(product.Name, saved.Name);
        Assert.AreEqual(product.Price, saved.Price);
    }
}