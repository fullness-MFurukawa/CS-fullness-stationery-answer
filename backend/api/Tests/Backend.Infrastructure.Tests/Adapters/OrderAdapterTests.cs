using Backend.Infrastructure.Adapters;
using EfOrder = Backend.Infrastructure.Entities.Order;
using DomainOrder = Backend.Domain.Models.Order;
using DomainCustomer = Backend.Domain.Models.Customer;
using DomainOrderStatus = Backend.Domain.Models.OrderStatus;
using DomainPaymentMethod = Backend.Domain.Models.PaymentMethod;
using DomainOrderDetail = Backend.Domain.Models.OrderDetail;
using DomainProduct = Backend.Domain.Models.Product;
using DomainProductCategory = Backend.Domain.Models.ProductCategory;
using DomainProductStock = Backend.Domain.Models.ProductStock;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class OrderAdapterTests
{
    private readonly OrderAdapter _adapter = new();

    /// <summary>
    /// テスト用の顧客（ドメイン）を生成する
    /// </summary>
    private static DomainCustomer CreateCustomer()
        => new(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", "テストビル101",
            "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.Now);

    /// <summary>
    /// テスト用の注文ステータス（ドメイン）を生成する
    /// </summary>
    private static DomainOrderStatus CreateStatus()
        => new(1, "注文済");

    /// <summary>
    /// テスト用の支払い方法（ドメイン）を生成する
    /// </summary>
    private static DomainPaymentMethod CreatePaymentMethod()
        => new(1, "現金");

    /// <summary>
    /// テスト用の商品（ドメイン）を生成する
    /// </summary>
    private static DomainProduct CreateProduct(int price = 120)
        => new(
            Guid.NewGuid(), "水性ボールペン(黒)", price, null,
            new DomainProductCategory(Guid.NewGuid(), "文房具"),
            new DomainProductStock(Guid.NewGuid(), 10));

    /// <summary>
    /// テスト用の注文明細（ドメイン）を生成する
    /// </summary>
    private static DomainOrderDetail CreateDetail(int id = 1, int price = 120, int count = 1)
        => new(id, CreateProduct(price), count);

    /// <summary>
    /// テスト用のEFエンティティを生成する
    /// </summary>
    private static EfOrder CreateEntity(Guid uuid, DateTime orderDate, int amountTotal)
        => new()
        {
            Id = 1,
            OrderUuid = uuid,
            OrderDate = orderDate,
            AmountTotal = amountTotal,
            CustomerId = 1,
            OrderStatusId = 1,
            PaymentMethodId = 1
        };

    [TestMethod(DisplayName = "EFエンティティと変換済みの関連からドメインエンティティを生成できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var customer = CreateCustomer();
        var status = CreateStatus();
        var paymentMethod = CreatePaymentMethod();
        var details = new[] { CreateDetail(1, 120, 2), CreateDetail(2, 100, 1) };
        var source = CreateEntity(uuid, orderDate, 340);

        var domain = _adapter.ToDomain(source, customer, status, paymentMethod, details);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual(orderDate, domain.OrderDate);
        Assert.AreEqual(340, domain.AmountTotal);
        Assert.AreEqual(customer, domain.Customer);
        Assert.AreEqual(status, domain.Status);
        Assert.AreEqual(paymentMethod, domain.PaymentMethod);
        Assert.HasCount(2, domain.Details);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var domain = new DomainOrder(
            uuid, orderDate, 340, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            new[] { CreateDetail() });

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.OrderUuid);
        Assert.AreEqual(orderDate, source.OrderDate);
        Assert.AreEqual(340, source.AmountTotal);
    }

    [TestMethod(DisplayName = "ToSourceでは主キーと外部キーを設定しない")]
    public void ToSource_DoesNotSetKeys()
    {
        var domain = new DomainOrder(
            Guid.NewGuid(), DateTime.Now, 340, CreateCustomer(), CreateStatus(), CreatePaymentMethod(),
            new[] { CreateDetail() });

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
        Assert.AreEqual(0, source.CustomerId);
        Assert.AreEqual(0, source.OrderStatusId);
        Assert.AreEqual(0, source.PaymentMethodId);
    }

    [TestMethod(DisplayName = "変換を往復しても各項目が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var orderDate = new DateTime(2026, 1, 1, 9, 0, 0);
        var customer = CreateCustomer();
        var status = CreateStatus();
        var paymentMethod = CreatePaymentMethod();
        var details = new[] { CreateDetail(1, 120, 2) };
        var original = new DomainOrder(uuid, orderDate, 240, customer, status, paymentMethod, details);

        var restored = _adapter.ToDomain(
            _adapter.ToSource(original), customer, status, paymentMethod, details);

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.OrderDate, restored.OrderDate);
        Assert.AreEqual(original.AmountTotal, restored.AmountTotal);
        Assert.HasCount(1, restored.Details);
    }
}