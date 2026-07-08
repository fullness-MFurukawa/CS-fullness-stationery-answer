using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class OrderTests
{
    private static Customer CreateCustomer()
        => new(Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", "テストビル101",
               "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.Now);

    private static OrderStatus CreateStatus()
        => new(1, "注文済");

    private static PaymentMethod CreatePaymentMethod()
        => new(1, "現金");

    private static Product CreateProduct(int price = 120)
        => new(Guid.NewGuid(), "水性ボールペン(黒)", price, null,
               new ProductCategory(Guid.NewGuid(), "文房具"),
               new ProductStock(Guid.NewGuid(), 10));

    private static OrderDetail CreateDetail(int id = 1, int price = 120, int count = 1)
        => new(id, CreateProduct(price), count);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var customer = CreateCustomer();
        var status = CreateStatus();
        var paymentMethod = CreatePaymentMethod();
        var details = new[] { CreateDetail(1, 120, 2), CreateDetail(2, 100, 1) };

        var order = new Order(id, orderDate, 340, customer, status, paymentMethod, details);

        Assert.AreEqual(id, order.Id);
        Assert.AreEqual(orderDate, order.OrderDate);
        Assert.AreEqual(340, order.AmountTotal);
        Assert.AreEqual(customer, order.Customer);
        Assert.AreEqual(status, order.Status);
        Assert.AreEqual(paymentMethod, order.PaymentMethod);
        Assert.AreEqual(2, order.Details.Count);
    }

    [TestMethod(DisplayName = "顧客が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullCustomer_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 120, null!, CreateStatus(), CreatePaymentMethod(),
            new[] { CreateDetail() }));
    }

    [TestMethod(DisplayName = "注文ステータスが未指定ならDomainExceptionをスローする")]
    public void Constructor_NullStatus_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 120, CreateCustomer(), null!, CreatePaymentMethod(),
            new[] { CreateDetail() }));
    }

    [TestMethod(DisplayName = "支払い方法が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullPaymentMethod_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 120, CreateCustomer(), CreateStatus(), null!,
            new[] { CreateDetail() }));
    }

    [TestMethod(DisplayName = "注文明細が未指定ならDomainExceptionをスローする")]
    public void Constructor_NullDetails_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 120, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            null!));
    }

    [TestMethod(DisplayName = "注文明細が空ならDomainExceptionをスローする")]
    public void Constructor_EmptyDetails_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, 120, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            Array.Empty<OrderDetail>()));
    }

    [TestMethod(DisplayName = "合計金額が負数ならDomainExceptionをスローする")]
    public void Constructor_NegativeAmountTotal_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.NewGuid(), DateTime.Now, -1, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            new[] { CreateDetail() }));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new Order(
            Guid.Empty, DateTime.Now, 120, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            new[] { CreateDetail() }));
    }
}