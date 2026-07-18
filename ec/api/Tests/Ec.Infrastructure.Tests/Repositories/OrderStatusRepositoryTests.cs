using Ec.Domain.Models;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class OrderStatusRepositoryTests : RepositoryTestBase
{
    private OrderStatusRepository CreateRepository()
        => new(Context, new OrderStatusAdapter());

    [TestMethod(DisplayName = "注文済のステータスをIDで取得できる")]
    public async Task FindByIdAsync_OrderedStatus_ReturnsStatus()
    {
        var repository = CreateRepository();

        // OrderStatus.OrderedId(=1) は「注文済」。
        // サンプルデータで先頭が「注文済」であることが前提
        var status = await repository.FindByIdAsync(OrderStatus.OrderedId);

        Assert.IsNotNull(status);
        Assert.AreEqual(OrderStatus.OrderedId, status!.Id);
        Assert.AreEqual("注文済", status.Name);
    }

    [TestMethod(DisplayName = "存在しないIDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var status = await repository.FindByIdAsync(9999);

        Assert.IsNull(status);
    }
}