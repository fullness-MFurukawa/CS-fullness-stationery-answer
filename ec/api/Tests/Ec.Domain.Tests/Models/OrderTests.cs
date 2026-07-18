using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class OrderTests
{
    /// <summary>
    /// テスト用の顧客を生成する
    /// </summary>
    private static Customer CreateCustomer()
        => new(
            Guid.NewGuid(),
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            null,
            "090-1234-5678",
            "test@example.com",
            "testuser",
            "hashed-password",
            DateTime.Now);

    /// <summary>
    /// テスト用の商品を生成する
    /// </summary>
    private static Product CreateProduct(string name = "水性ボールペン(黒)", int price = 150)
        => new(
            Guid.NewGuid(),
            name,
            price,
            null,
            new ProductCategory(Guid.NewGuid(), "文房具"),
            new ProductStock(Guid.NewGuid(), 100));

    /// <summary>
    /// テスト用の注文ステータス（注文済）を生成する
    /// </summary>
    private static OrderStatus CreateOrderedStatus()
        => new(OrderStatus.OrderedId, "注文済");

    /// <summary>
    /// テスト用の支払い方法を生成する
    /// </summary>
    private static PaymentMethod CreatePaymentMethod()
        => new(1, "現金");

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var customer = CreateCustomer();
        var status = CreateOrderedStatus();
        var paymentMethod = CreatePaymentMethod();
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        var order = new Order(id, orderDate, 450, customer, status, paymentMethod, details);

        Assert.AreEqual(id, order.Id);
        Assert.AreEqual(orderDate, order.OrderDate);
        Assert.AreEqual(450, order.AmountTotal);
        Assert.AreEqual(customer, order.Customer);
        Assert.AreEqual(status, order.Status);
        Assert.AreEqual(paymentMethod, order.PaymentMethod);
        Assert.HasCount(1, order.Details);
    }

    [TestMethod(DisplayName = "顧客が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingCustomer_ThrowsDomainException()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 450, null!, CreateOrderedStatus(), CreatePaymentMethod(), details));
    }

    [TestMethod(DisplayName = "注文ステータスが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingStatus_ThrowsDomainException()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 450, CreateCustomer(), null!, CreatePaymentMethod(), details));
    }

    [TestMethod(DisplayName = "支払い方法が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingPaymentMethod_ThrowsDomainException()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 450, CreateCustomer(), CreateOrderedStatus(), null!, details));
    }

    [TestMethod(DisplayName = "注文明細が未指定または空ならDomainExceptionをスローする")]
    public void Constructor_EmptyDetails_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 0, CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), null!));
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 0, CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), []));
    }

    [TestMethod(DisplayName = "合計金額が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativeAmountTotal_ThrowsDomainException()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, -1, CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), details));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.Empty, DateTime.Now, 450, CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), details));
    }

    [TestMethod(DisplayName = "生成すると合計金額が注文明細の小計から算出される")]
    public void Create_CalculatesAmountTotalFromDetails()
    {
        var details = new List<OrderDetail>
        {
            new(CreateProduct(name: "商品A", price: 150), 3),   // 450
            new(CreateProduct(name: "商品B", price: 200), 2),   // 400
        };

        var order = Order.Create(
            Guid.NewGuid(), CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), details, DateTime.Now);

        // 呼び出し側に合計金額を渡させると、明細と合わない値を渡せてしまう。
        // ドメインが自ら計算することで、必ず一致する
        Assert.AreEqual(850, order.AmountTotal);
    }

    [TestMethod(DisplayName = "生成すると注文済のステータスが設定される")]
    public void Create_SetsOrderedStatus()
    {
        var status = CreateOrderedStatus();
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        var order = Order.Create(
            Guid.NewGuid(), CreateCustomer(), status, CreatePaymentMethod(), details, DateTime.Now);

        Assert.AreEqual(OrderStatus.OrderedId, order.Status.Id);
        Assert.AreEqual("注文済", order.Status.Name);
    }

    [TestMethod(DisplayName = "生成すると指定した値が各プロパティに設定される")]
    public void Create_SetsProperties()
    {
        var id = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var customer = CreateCustomer();
        var paymentMethod = CreatePaymentMethod();
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };

        var order = Order.Create(id, customer, CreateOrderedStatus(), paymentMethod, details, orderDate);

        Assert.AreEqual(id, order.Id);
        Assert.AreEqual(orderDate, order.OrderDate);
        Assert.AreEqual(customer, order.Customer);
        Assert.AreEqual(paymentMethod, order.PaymentMethod);
        Assert.HasCount(1, order.Details);
    }

    [TestMethod(DisplayName = "同じ商品が複数の注文明細に含まれるとDomainExceptionをスローする")]
    public void Create_DuplicatedProduct_ThrowsDomainException()
    {
        var product = CreateProduct();
        var details = new List<OrderDetail>
        {
            new(product, 1),
            new(product, 2),
        };

        // 同じ商品が複数行に分かれていると、在庫の引き当てが行ごとに実行され、
        // 悲観的ロックを行っても意図しない結果になりうる
        Assert.ThrowsExactly<DomainException>(() => Order.Create(
            Guid.NewGuid(), CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), details, DateTime.Now));
    }

    [TestMethod(DisplayName = "注文明細が空なら生成時にDomainExceptionをスローする")]
    public void Create_EmptyDetails_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => Order.Create(
            Guid.NewGuid(), CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), [], DateTime.Now));
        Assert.ThrowsExactly<DomainException>(() => Order.Create(
            Guid.NewGuid(), CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), null!, DateTime.Now));
    }

    [TestMethod(DisplayName = "注文明細は生成後に外部から変更できない")]
    public void Details_IsIsolatedFromCaller()
    {
        var details = new List<OrderDetail> { new(CreateProduct(), 3) };
        var order = Order.Create(
            Guid.NewGuid(), CreateCustomer(), CreateOrderedStatus(), CreatePaymentMethod(), details, DateTime.Now);

        // 呼び出し側が元のListを変更しても、注文の明細には影響しない
        details.Add(new OrderDetail(CreateProduct(name: "商品C"), 1));

        Assert.HasCount(1, order.Details);
    }
}