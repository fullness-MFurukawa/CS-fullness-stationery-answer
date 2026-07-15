using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Factories;
using Backend.Infrastructure.Repositories;

namespace Backend.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Backend.Infrastructure.Repositories")]
public class OrderRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象のリポジトリを生成する（基底クラスのコンテキストを共有する）
    /// </summary>
    private OrderRepository CreateRepository()
    {
        var productFactory = new ProductFactory(
            new ProductCategoryAdapter(), new ProductStockAdapter(), new ProductAdapter());

        var orderFactory = new OrderFactory(
            new CustomerAdapter(),
            new OrderStatusAdapter(),
            new PaymentMethodAdapter(),
            productFactory,
            new OrderDetailAdapter(),
            new OrderAdapter());

        return new OrderRepository(Context, orderFactory);
    }

    /// <summary>
    /// 注文ステータスのリポジトリを生成する
    /// </summary>
    private OrderStatusRepository CreateOrderStatusRepository()
        => new(Context, new OrderStatusAdapter());

    [TestMethod(DisplayName = "すべての注文を新しい順に取得できる")]
    public async Task FindAllAsync_ReturnsAllOrdersNewestFirst()
    {
        var repository = CreateRepository();

        var orders = await repository.FindAllAsync();

        Assert.HasCount(4, orders);
        Assert.AreEqual(3800, orders[0].AmountTotal);
        Assert.AreEqual(120, orders[1].AmountTotal);
        Assert.AreEqual(100, orders[2].AmountTotal);
        Assert.AreEqual(340, orders[3].AmountTotal);
    }

    [TestMethod(DisplayName = "取得した注文に顧客・ステータス・支払い方法が含まれる")]
    public async Task FindAllAsync_IncludesCustomerStatusAndPaymentMethod()
    {
        var repository = CreateRepository();

        var order = (await repository.FindAllAsync())[0];

        Assert.AreEqual("taro123", order.Customer.Username);
        Assert.AreEqual("山田太郎", order.Customer.Name);
        Assert.AreEqual("注文済", order.Status.Name);
        Assert.AreEqual("現金", order.PaymentMethod.Name);
    }

    [TestMethod(DisplayName = "取得した注文に明細と商品が含まれ小計が算出される")]
    public async Task FindAllAsync_IncludesOrderDetailsWithProduct()
    {
        var repository = CreateRepository();

        var order = (await repository.FindAllAsync())[0];
        var detail = order.Details[0];

        Assert.HasCount(1, order.Details);
        Assert.AreEqual("有線ゲーミングマウス", detail.Product.Name);
        Assert.AreEqual("パソコン周辺機器", detail.Product.Category.Name);
        Assert.AreEqual(1, detail.Count);
        Assert.AreEqual(3800, detail.Subtotal);
    }

    [TestMethod(DisplayName = "明細が複数ある注文を取得できる")]
    public async Task FindAllAsync_OrderWithMultipleDetails_IsAssembled()
    {
        var repository = CreateRepository();

        var order = (await repository.FindAllAsync())[3];

        Assert.AreEqual(340, order.AmountTotal);
        Assert.HasCount(2, order.Details);
        Assert.AreEqual(340, order.Details.Sum(d => d.Subtotal));
    }

    [TestMethod(DisplayName = "購入日を指定して注文を検索できる")]
    public async Task SearchAsync_ByOrderDate_ReturnsMatchingOrders()
    {
        var repository = CreateRepository();

        var orders = await repository.SearchAsync(new DateOnly(2024, 5, 12), null);

        Assert.HasCount(1, orders);
        Assert.AreEqual(100, orders[0].AmountTotal);
        Assert.AreEqual("配送中", orders[0].Status.Name);
    }

    [TestMethod(DisplayName = "顧客アカウント名を指定して注文を検索できる")]
    public async Task SearchAsync_ByCustomerAccountName_ReturnsMatchingOrders()
    {
        var repository = CreateRepository();

        var orders = await repository.SearchAsync(null, "taro123");

        Assert.HasCount(1, orders);
        Assert.AreEqual(3800, orders[0].AmountTotal);
        Assert.AreEqual("山田太郎", orders[0].Customer.Name);
    }

    [TestMethod(DisplayName = "購入日と顧客アカウント名の両方を指定して注文を検索できる")]
    public async Task SearchAsync_ByBothConditions_ReturnsMatchingOrders()
    {
        var repository = CreateRepository();

        var orders = await repository.SearchAsync(new DateOnly(2024, 5, 12), "testuser");

        Assert.HasCount(1, orders);
        Assert.AreEqual(100, orders[0].AmountTotal);
    }

    [TestMethod(DisplayName = "検索条件を指定しない場合はすべての注文を取得する")]
    public async Task SearchAsync_WithoutConditions_ReturnsAllOrders()
    {
        var repository = CreateRepository();

        var orders = await repository.SearchAsync(null, null);

        Assert.HasCount(4, orders);
    }

    [TestMethod(DisplayName = "条件に一致する注文がない場合は空の一覧を返す")]
    public async Task SearchAsync_NoMatch_ReturnsEmpty()
    {
        var repository = CreateRepository();

        var orders = await repository.SearchAsync(new DateOnly(2020, 1, 1), null);

        Assert.HasCount(0, orders);
    }

    [TestMethod(DisplayName = "識別IDを指定して注文を取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsOrder()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync())[0];

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected, actual);
        Assert.AreEqual(3800, actual.AmountTotal);
        Assert.HasCount(1, actual.Details);
    }

    [TestMethod(DisplayName = "存在しない識別IDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(actual);
    }

    [TestMethod(DisplayName = "注文ステータスを更新できる")]
    public async Task UpdateStatusAsync_ExistingOrder_UpdatesStatus()
    {
        var repository = CreateRepository();
        var statusRepository = CreateOrderStatusRepository();
        var target = (await repository.FindAllAsync())[0];
        var newStatus = await statusRepository.FindByIdAsync(2);

        Assert.IsNotNull(newStatus);
        Assert.AreEqual("注文済", target.Status.Name);

        await repository.UpdateStatusAsync(target.Id, newStatus);

        var updated = await repository.FindByIdAsync(target.Id);
        Assert.IsNotNull(updated);
        Assert.AreEqual(2, updated.Status.Id);
        Assert.AreEqual("入金済", updated.Status.Name);
    }

    [TestMethod(DisplayName = "注文の件数を取得できる")]
    [TestCategory("Backend.Infrastructure.Repositories")]
    public async Task CountAsync_ReturnsOrderCount()
    {
        var repository = CreateRepository();

        var count = await repository.CountAsync();

        Assert.AreEqual(4, count);
    }

    [TestMethod(DisplayName = "すべての注文の合計金額を集計できる")]
    [TestCategory("Backend.Infrastructure.Repositories")]
    public async Task SumAmountTotalAsync_ReturnsTotalSales()
    {
        var repository = CreateRepository();

        var total = await repository.SumAmountTotalAsync();

        // 340 + 100 + 120 + 3800
        Assert.AreEqual(4360, total);
    }

    [TestMethod(DisplayName = "注文ステータスごとの件数を集計できる")]
    [TestCategory("Backend.Infrastructure.Repositories")]
    public async Task CountByStatusAsync_ReturnsCountsByStatus()
    {
        var repository = CreateRepository();

        var counts = await repository.CountByStatusAsync();

        // 注文済:1件、配送中:1件、完了:2件
        Assert.AreEqual(1, counts[1]);
        Assert.AreEqual(1, counts[3]);
        Assert.AreEqual(2, counts[4]);
    }

    [TestMethod(DisplayName = "該当する注文が無いステータスは集計結果に含まれない")]
    [TestCategory("Backend.Infrastructure.Repositories")]
    public async Task CountByStatusAsync_StatusWithoutOrders_IsNotIncluded()
    {
        var repository = CreateRepository();

        var counts = await repository.CountByStatusAsync();

        // 入金済(ID:2)の注文は存在しない
        Assert.IsFalse(counts.ContainsKey(2));
    }
}