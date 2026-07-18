using Ec.Domain.Exceptions;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Factories;

using EfCustomer = Ec.Infrastructure.Entities.Customer;
using EfOrder = Ec.Infrastructure.Entities.Order;
using EfOrderDetail = Ec.Infrastructure.Entities.OrderDetail;
using EfOrderStatus = Ec.Infrastructure.Entities.OrderStatus;
using EfPaymentMethod = Ec.Infrastructure.Entities.PaymentMethod;
using EfProduct = Ec.Infrastructure.Entities.Product;
using EfProductCategory = Ec.Infrastructure.Entities.ProductCategory;
using EfProductStock = Ec.Infrastructure.Entities.ProductStock;

namespace Ec.Infrastructure.Tests.Factories;

[TestClass]
[TestCategory("Backend.Infrastructure.Factories")]
public class OrderFactoryTests
{
    private readonly OrderFactory _factory = new(
        new CustomerAdapter(),
        new OrderStatusAdapter(),
        new PaymentMethodAdapter(),
        new ProductFactory(new ProductCategoryAdapter(), new ProductStockAdapter(), new ProductAdapter()),
        new OrderDetailAdapter(),
        new OrderAdapter());

    /// <summary>
    /// テスト用のEF商品エンティティを生成する
    /// </summary>
    private static EfProduct CreateProductEntity(bool withCategory = true, bool withStock = true)
    {
        var product = new EfProduct
        {
            Id = 1,
            ProductUuid = Guid.NewGuid(),
            Name = "水性ボールペン(黒)",
            Price = 120,
            ImageUrl = null,
            ProductCategoryId = 1,
            DeleteFlg = 0
        };

        if (withCategory)
        {
            product.Category = new EfProductCategory { Id = 1, CategoryUuid = Guid.NewGuid(), Name = "文房具" };
        }
        if (withStock)
        {
            product.Stock = new EfProductStock { Id = 1, StockUuid = Guid.NewGuid(), Quantity = 10, ProductId = 1 };
        }

        return product;
    }

    /// <summary>
    /// テスト用のEF注文明細エンティティを生成する
    /// </summary>
    private static EfOrderDetail CreateDetailEntity(
        bool withProduct = true,
        bool withCategory = true,
        bool withStock = true)
    {
        var detail = new EfOrderDetail
        {
            Id = 1,
            OrderId = 1,
            ProductId = 1,
            Count = 2
        };

        if (withProduct)
        {
            detail.Product = CreateProductEntity(withCategory, withStock);
        }

        return detail;
    }

    /// <summary>
    /// テスト用のEF注文エンティティを生成する
    /// </summary>
    private static EfOrder CreateEntity(
        bool withCustomer = true,
        bool withStatus = true,
        bool withPaymentMethod = true,
        bool withDetails = true,
        bool withDetailProduct = true,
        bool withProductCategory = true,
        bool withProductStock = true)
    {
        var order = new EfOrder
        {
            Id = 1,
            OrderUuid = Guid.NewGuid(),
            OrderDate = new DateTime(2026, 1, 1, 9, 0, 0),
            AmountTotal = 240,
            CustomerId = 1,
            OrderStatusId = 1,
            PaymentMethodId = 1
        };

        if (withCustomer)
        {
            order.Customer = new EfCustomer
            {
                Id = 1,
                CustomerUuid = Guid.NewGuid(),
                Name = "テスト顧客",
                NameKana = "テストコキャク",
                Address1 = "東京都新宿区",
                Address2 = "テストビル101",
                PhoneNumber = "090-1234-5678",
                MailAddress = "test@example.com",
                Username = "testuser",
                Password = "hashed-password",
                CreatedAt = DateTime.Now
            };
        }
        if (withStatus)
        {
            order.OrderStatus = new EfOrderStatus { Id = 1, Name = "注文済" };
        }
        if (withPaymentMethod)
        {
            order.PaymentMethod = new EfPaymentMethod { Id = 1, Name = "現金" };
        }
        if (withDetails)
        {
            order.OrderDetails.Add(CreateDetailEntity(withDetailProduct, withProductCategory, withProductStock));
        }

        return order;
    }

    [TestMethod(DisplayName = "関連がすべてロード済みなら注文集約を組み立てられる")]
    public void Create_WithLoadedRelations_ReturnsOrderAggregate()
    {
        var entity = CreateEntity();

        var order = _factory.Create(entity);

        Assert.AreEqual(entity.OrderUuid, order.Id);
        Assert.AreEqual(new DateTime(2026, 1, 1, 9, 0, 0), order.OrderDate);
        Assert.AreEqual(240, order.AmountTotal);
        Assert.HasCount(1, order.Details);
    }

    [TestMethod(DisplayName = "組み立てた注文集約に顧客・ステータス・支払い方法が含まれる")]
    public void Create_WithLoadedRelations_IncludesRelatedEntities()
    {
        var entity = CreateEntity();

        var order = _factory.Create(entity);

        Assert.AreEqual(entity.Customer.CustomerUuid, order.Customer.Id);
        Assert.AreEqual("testuser", order.Customer.Username);
        Assert.AreEqual(1, order.Status.Id);
        Assert.AreEqual("注文済", order.Status.Name);
        Assert.AreEqual(1, order.PaymentMethod.Id);
        Assert.AreEqual("現金", order.PaymentMethod.Name);
    }

    [TestMethod(DisplayName = "注文明細に商品が含まれ小計が算出される")]
    public void Create_WithLoadedRelations_DetailHasProductAndSubtotal()
    {
        var entity = CreateEntity();

        var order = _factory.Create(entity);
        var detail = order.Details[0];

        Assert.AreEqual("水性ボールペン(黒)", detail.Product.Name);
        Assert.AreEqual(2, detail.Count);
        Assert.AreEqual(240, detail.Subtotal);
    }

    [TestMethod(DisplayName = "顧客が未ロードならDomainExceptionをスローする")]
    public void Create_CustomerNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withCustomer: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "注文ステータスが未ロードならDomainExceptionをスローする")]
    public void Create_OrderStatusNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withStatus: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "支払い方法が未ロードならDomainExceptionをスローする")]
    public void Create_PaymentMethodNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withPaymentMethod: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "注文明細が未ロード（0件）ならDomainExceptionをスローする")]
    public void Create_OrderDetailsNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withDetails: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "注文明細の商品が未ロードならDomainExceptionをスローする")]
    public void Create_DetailProductNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withDetailProduct: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "商品のカテゴリが未ロードならDomainExceptionをスローする")]
    public void Create_ProductCategoryNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withProductCategory: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }

    [TestMethod(DisplayName = "商品の在庫が未ロードならDomainExceptionをスローする")]
    public void Create_ProductStockNotLoaded_ThrowsDomainException()
    {
        var entity = CreateEntity(withProductStock: false);

        Assert.ThrowsExactly<DomainException>(() => _factory.Create(entity));
    }
}