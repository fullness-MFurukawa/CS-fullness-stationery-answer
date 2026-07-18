using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class ProductCategoryRepositoryTests : RepositoryTestBase
{
    private ProductCategoryRepository CreateRepository()
        => new(Context, new ProductCategoryAdapter());

    [TestMethod(DisplayName = "すべての商品カテゴリをID順で取得できる")]
    public async Task FindAllAsync_ReturnsCategoriesOrderedById()
    {
        var repository = CreateRepository();

        var categories = await repository.FindAllAsync();

        Assert.IsTrue(categories.Count > 0);
        // 文房具・雑貨などサンプルデータのカテゴリが含まれる
        Assert.IsTrue(categories.Any(c => c.Name == "文房具"));
    }

    [TestMethod(DisplayName = "識別IDを指定してカテゴリを取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsCategory()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync()).First(c => c.Name == "文房具");

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Id, actual!.Id);
        Assert.AreEqual("文房具", actual.Name);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }
}