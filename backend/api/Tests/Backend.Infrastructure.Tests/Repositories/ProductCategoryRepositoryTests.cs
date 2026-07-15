using Backend.Domain.Models;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class ProductCategoryRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private ProductCategoryRepository CreateRepository()
        => new(Context, new ProductCategoryAdapter());

    [TestMethod(DisplayName = "登録済みの商品カテゴリをカテゴリID順にすべて取得できる")]
    public async Task FindAllAsync_ReturnsSeededCategoriesOrderedById()
    {
        var repository = CreateRepository();

        var categories = await repository.FindAllAsync();

        Assert.HasCount(3, categories);
        Assert.AreEqual("文房具", categories[0].Name);
        Assert.AreEqual("パソコン周辺機器", categories[1].Name);
        Assert.AreEqual("雑貨", categories[2].Name);
    }

    [TestMethod(DisplayName = "識別IDを指定して商品カテゴリを取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsCategory()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(expected.Name, actual.Name);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }

    [TestMethod(DisplayName = "商品カテゴリを新規登録すると識別IDで取得できる")]
    public async Task AddAsync_NewCategory_CanBeFoundById()
    {
        var repository = CreateRepository();
        var newCategory = new ProductCategory(Guid.NewGuid(), "テストカテゴリ");

        await repository.AddAsync(newCategory);

        var saved = await repository.FindByIdAsync(newCategory.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual(newCategory.Id, saved.Id);
        Assert.AreEqual("テストカテゴリ", saved.Name);
    }

    [TestMethod(DisplayName = "商品カテゴリを新規登録すると件数が1件増える")]
    public async Task AddAsync_NewCategory_IncreasesCount()
    {
        var repository = CreateRepository();
        var before = await repository.FindAllAsync();

        await repository.AddAsync(new ProductCategory(Guid.NewGuid(), "テストカテゴリ"));

        var after = await repository.FindAllAsync();
        Assert.HasCount(before.Count + 1, after);
    }

    [TestMethod(DisplayName = "商品カテゴリの件数を取得できる")]
    [TestCategory("Backend.Infrastructure.Repositories")]
    public async Task CountAsync_ReturnsCategoryCount()
    {
        var repository = CreateRepository();
       
        var count = await repository.CountAsync();

        Assert.AreEqual(3, count);
    }
}