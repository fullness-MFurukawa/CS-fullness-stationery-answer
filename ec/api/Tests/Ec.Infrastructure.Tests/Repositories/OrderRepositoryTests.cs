using Ec.Domain.Models;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Factories;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class OrderRepositoryTests : RepositoryTestBase
{
    /// <summary>
    /// テスト対象の注文リポジトリを生成する
    /// </summary>
    private OrderRepository CreateOrderRepository()
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

    private ProductRepository CreateProductRepository()
        => new(
            Context,
            new ProductStockAdapter(),
            new ProductFactory(new ProductCategoryAdapter(), new ProductStockAdapter(), new ProductAdapter()));

    private CustomerRepository CreateCustomerRepository()
        => new(Context, new CustomerAdapter());

    private OrderStatusRepository CreateOrderStatusRepository()
        => new(Context, new OrderStatusAdapter());

    private PaymentMethodRepository CreatePaymentMethodRepository()
        => new(Context, new PaymentMethodAdapter());

    /// <summary>
    /// テスト用の顧客を登録して返す
    /// </summary>
    private async Task<Customer> RegisterCustomerAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = new Customer(
            Guid.NewGuid(),
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            "テストビル101",
            "090-1234-5678",
            $"order_{suffix}@example.com",
            $"order_{suffix}",
            "hashed-password",
            DateTime.Now);
        await CreateCustomerRepository().AddAsync(customer);
        return customer;
    }

    /// <summary>
    /// テスト用の注文を組み立てる（既存の商品と登録済みの顧客を使う）
    /// </summary>
    private async Task<Order> BuildOrderAsync(Customer customer, params (Product product, int count)[] items)
    {
        var orderedStatus = await CreateOrderStatusRepository().FindByIdAsync(OrderStatus.OrderedId);
        var paymentMethod = (await CreatePaymentMethodRepository().FindAllAsync()).First(m => m.Name == "現金");
        var details = items.Select(i => new OrderDetail(i.product, i.count)).ToList();
        return Order.Create(Guid.NewGuid(), customer, orderedStatus!, paymentMethod, details, DateTime.Now);
    }

    [TestMethod(DisplayName = "注文を明細ごと登録し、識別IDで取得できる")]
    public async Task AddAsync_ThenFindById_ReturnsOrderWithDetails()
    {
        var customer = await RegisterCustomerAsync();
        var products = await CreateProductRepository().FindAllAsync();
        var order = await BuildOrderAsync(customer, (products[0], 2), (products[1], 1));

        await CreateOrderRepository().AddAsync(order);
        var saved = await CreateOrderRepository().FindByIdAsync(order.Id, customer.Id);

        Assert.IsNotNull(saved);
        Assert.AreEqual(order.Id, saved!.Id);
        Assert.HasCount(2, saved.Details);
        Assert.AreEqual(customer.Id, saved.Customer.Id);
        Assert.AreEqual("注文済", saved.Status.Name);
        Assert.AreEqual("現金", saved.PaymentMethod.Name);
        // 合計金額が明細から算出された値と一致する
        var expectedTotal = products[0].Price * 2 + products[1].Price * 1;
        Assert.AreEqual(expectedTotal, saved.AmountTotal);
    }

    [TestMethod(DisplayName = "顧客を指定して注文履歴を取得できる")]
    public async Task FindByCustomerAsync_ReturnsCustomerOrders()
    {
        var customer = await RegisterCustomerAsync();
        var products = await CreateProductRepository().FindAllAsync();
        await CreateOrderRepository().AddAsync(await BuildOrderAsync(customer, (products[0], 1)));
        await CreateOrderRepository().AddAsync(await BuildOrderAsync(customer, (products[1], 1)));

        var orders = await CreateOrderRepository().FindByCustomerAsync(customer.Id);

        Assert.HasCount(2, orders);
        Assert.IsTrue(orders.All(o => o.Customer.Id == customer.Id));
    }

    [TestMethod(DisplayName = "他人の顧客IDを指定した注文はnullを返す")]
    public async Task FindByIdAsync_OtherCustomer_ReturnsNull()
    {
        var owner = await RegisterCustomerAsync();
        var other = await RegisterCustomerAsync();
        var products = await CreateProductRepository().FindAllAsync();
        var order = await BuildOrderAsync(owner, (products[0], 1));
        await CreateOrderRepository().AddAsync(order);

        // 別の顧客のIDで取得を試みる
        var saved = await CreateOrderRepository().FindByIdAsync(order.Id, other.Id);

        // 他人の注文は取得できない（UC007のアクセス制御）
        Assert.IsNull(saved);
    }

    [TestMethod(DisplayName = "存在しない注文IDはnullを返す")]
    public async Task FindByIdAsync_NotExisting_ReturnsNull()
    {
        var customer = await RegisterCustomerAsync();

        var saved = await CreateOrderRepository().FindByIdAsync(Guid.NewGuid(), customer.Id);

        Assert.IsNull(saved);
    }

    [TestMethod(DisplayName = "注文履歴がない顧客は空の一覧を返す")]
    public async Task FindByCustomerAsync_NoOrders_ReturnsEmpty()
    {
        var customer = await RegisterCustomerAsync();

        var orders = await CreateOrderRepository().FindByCustomerAsync(customer.Id);

        Assert.IsEmpty(orders);
    }
}