using Backend.Infrastructure.Adapters;

using DomainProductStock = Backend.Domain.Models.ProductStock;
using EfProductStock = Backend.Infrastructure.Entities.ProductStock;

namespace Backend.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class ProductStockAdapterTests
{
    private readonly ProductStockAdapter _adapter = new();

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var source = new EfProductStock
        {
            Id = 1,
            StockUuid = uuid,
            Quantity = 10,
            ProductId = 5
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual(10, domain.Quantity);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainProductStock(uuid, 10);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.StockUuid);
        Assert.AreEqual(10, source.Quantity);
    }

    [TestMethod(DisplayName = "ToSourceではDB採番の主キーと外部キーを設定しない")]
    public void ToSource_DoesNotSetDatabaseGeneratedIdAndForeignKey()
    {
        var domain = new DomainProductStock(Guid.NewGuid(), 10);

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
        Assert.AreEqual(0, source.ProductId);
    }

    [TestMethod(DisplayName = "在庫数が0でも変換できる")]
    public void ToDomain_ZeroQuantity_ReturnsDomainEntity()
    {
        var source = new EfProductStock
        {
            Id = 1,
            StockUuid = Guid.NewGuid(),
            Quantity = 0,
            ProductId = 5
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(0, domain.Quantity);
    }

    [TestMethod(DisplayName = "変換を往復しても識別IDと在庫数が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var original = new DomainProductStock(uuid, 10);

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Quantity, restored.Quantity);
    }
}