using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class OrderStatusRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private OrderStatusRepository CreateRepository()
        => new(Context, new OrderStatusAdapter());

    [TestMethod(DisplayName = "すべての注文ステータスをID昇順で取得できる")]
    public async Task FindAllAsync_ReturnsAllStatusesOrderedById()
    {
        var repository = CreateRepository();

        var statuses = await repository.FindAllAsync();

        Assert.HasCount(4, statuses);
        Assert.AreEqual(1, statuses[0].Id);
        Assert.AreEqual("注文済", statuses[0].Name);
        Assert.AreEqual(2, statuses[1].Id);
        Assert.AreEqual("入金済", statuses[1].Name);
        Assert.AreEqual(3, statuses[2].Id);
        Assert.AreEqual("配送中", statuses[2].Name);
        Assert.AreEqual(4, statuses[3].Id);
        Assert.AreEqual("完了", statuses[3].Name);
    }

    [TestMethod(DisplayName = "IDを指定して注文ステータスを取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsStatus()
    {
        var repository = CreateRepository();

        var status = await repository.FindByIdAsync(3);

        Assert.IsNotNull(status);
        Assert.AreEqual(3, status.Id);
        Assert.AreEqual("配送中", status.Name);
    }

    [TestMethod(DisplayName = "存在しないIDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var status = await repository.FindByIdAsync(999);

        Assert.IsNull(status);
    }
}