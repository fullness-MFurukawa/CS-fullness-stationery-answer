using Backend.Application.Interactor;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class OrderStatusSearchInteractorTests
{
    private Mock<IOrderStatusRepository> _orderStatusRepository = null!;
    private OrderStatusSearchInteractor _interactor = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _orderStatusRepository = new Mock<IOrderStatusRepository>();
        _interactor = new OrderStatusSearchInteractor(_orderStatusRepository.Object);
    }

    [TestMethod(DisplayName = "すべての注文ステータスを取得する")]
    public async Task ExecuteAsync_ReturnsAllOrderStatuses()
    {
        IReadOnlyList<OrderStatus> expected =
        [
            new(1, "注文済"),
            new(2, "入金済"),
            new(3, "配送中"),
            new(4, "完了")
        ];
        _orderStatusRepository.Setup(r => r.FindAllAsync()).ReturnsAsync(expected);

        var statuses = await _interactor.ExecuteAsync();

        Assert.HasCount(4, statuses);
        Assert.AreEqual(1, statuses[0].Id);
        Assert.AreEqual("注文済", statuses[0].Name);
        Assert.AreEqual(4, statuses[3].Id);
        Assert.AreEqual("完了", statuses[3].Name);
    }

    [TestMethod(DisplayName = "リポジトリの取得処理を1回だけ呼び出す")]
    public async Task ExecuteAsync_CallsRepositoryOnce()
    {
        _orderStatusRepository.Setup(r => r.FindAllAsync()).ReturnsAsync([]);

        await _interactor.ExecuteAsync();

        _orderStatusRepository.Verify(r => r.FindAllAsync(), Times.Once);
    }

    [TestMethod(DisplayName = "注文ステータスが0件でも例外にせず空の一覧を返す")]
    public async Task ExecuteAsync_NoStatuses_ReturnsEmptyList()
    {
        _orderStatusRepository.Setup(r => r.FindAllAsync()).ReturnsAsync([]);

        var statuses = await _interactor.ExecuteAsync();

        Assert.HasCount(0, statuses);
    }
}