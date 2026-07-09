using Backend.Domain.Models;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Factories;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class ProductRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private ProductRepository CreateRepository()
        => new(
            Context,
            new ProductAdapter(),
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
        Assert.AreEqual(10, first.Stock.Quantity);
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

    [TestMethod(DisplayName = "識別IDを指定して商品を取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsProduct()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual("水性ボールペン(黒)", actual.Name);
        Assert.AreEqual(120, actual.Price);
        Assert.AreEqual("文房具", actual.Category.Name);
        Assert.AreEqual(10, actual.Stock.Quantity);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }

    [TestMethod(DisplayName = "商品を新規登録すると在庫とカテゴリごと取得できる")]
    public async Task AddAsync_NewProduct_PersistsProductWithStockAndCategory()
    {
        var repository = CreateRepository();
        var category = await GetCategoryAsync("雑貨");
        var newProduct = new Product(
            Guid.NewGuid(), "テスト商品", 500, null, category, new ProductStock(Guid.NewGuid(), 20));

        await repository.AddAsync(newProduct);

        var saved = await repository.FindByIdAsync(newProduct.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual("テスト商品", saved.Name);
        Assert.AreEqual(500, saved.Price);
        Assert.AreEqual("雑貨", saved.Category.Name);
        Assert.AreEqual(20, saved.Stock.Quantity);
        Assert.IsFalse(saved.IsDeleted);
    }

    [TestMethod(DisplayName = "商品を新規登録すると有効な商品の件数が1件増える")]
    public async Task AddAsync_NewProduct_IncreasesActiveCount()
    {
        var repository = CreateRepository();
        var category = await GetCategoryAsync("雑貨");
        var before = await repository.FindAllAsync();

        await repository.AddAsync(new Product(
            Guid.NewGuid(), "テスト商品", 500, null, category, new ProductStock(Guid.NewGuid(), 20)));

        var after = await repository.FindAllAsync();
        Assert.HasCount(before.Count + 1, after);
    }

    [TestMethod(DisplayName = "商品情報と在庫数を更新できる")]
    public async Task UpdateAsync_ExistingProduct_UpdatesFieldsAndStock()
    {
        var repository = CreateRepository();
        var original = (await repository.FindAllAsync())[0];
        var updated = new Product(
            original.Id,
            "水性ボールペン(黒) 改",
            150,
            "https://example.com/pen.png",
            original.Category,
            new ProductStock(original.Stock.Id, 55));

        await repository.UpdateAsync(updated);

        var saved = await repository.FindByIdAsync(original.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual("水性ボールペン(黒) 改", saved.Name);
        Assert.AreEqual(150, saved.Price);
        Assert.AreEqual("https://example.com/pen.png", saved.ImageUrl);
        Assert.AreEqual(55, saved.Stock.Quantity);
    }

    [TestMethod(DisplayName = "商品を削除すると論理削除され有効な商品から除外される")]
    public async Task DeleteByIdAsync_ExistingProduct_IsExcludedFromActiveProducts()
    {
        var repository = CreateRepository();
        var target = (await repository.FindAllAsync())[0];

        await repository.DeleteByIdAsync(target.Id);

        var products = await repository.FindAllAsync();
        Assert.HasCount(12, products);
        Assert.IsFalse(products.Any(p => p.Id == target.Id));
    }

    [TestMethod(DisplayName = "論理削除された商品は物理削除されずIsDeletedがtrueで残る")]
    public async Task DeleteByIdAsync_ExistingProduct_IsNotPhysicallyDeleted()
    {
        var repository = CreateRepository();
        var target = (await repository.FindAllAsync())[0];

        await repository.DeleteByIdAsync(target.Id);

        var saved = await repository.FindByIdAsync(target.Id);
        Assert.IsNotNull(saved);
        Assert.IsTrue(saved.IsDeleted);
        Assert.AreEqual(target.Name, saved.Name);
    }
}