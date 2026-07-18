using Ec.Infrastructure.Adapters;

using DomainProductCategory = Ec.Domain.Models.ProductCategory;
using EfProductCategory = Ec.Infrastructure.Entities.ProductCategory;

namespace Ec.Infrastructure.Tests.Adapters;

[TestClass]
[TestCategory("Backend.Infrastructure.Adapters")]
public class ProductCategoryAdapterTests
{
    private readonly ProductCategoryAdapter _adapter = new();

    [TestMethod(DisplayName = "EFエンティティからドメインエンティティへ変換できる")]
    public void ToDomain_ValidEntity_ReturnsDomainEntity()
    {
        var uuid = Guid.NewGuid();
        var source = new EfProductCategory
        {
            Id = 1,
            CategoryUuid = uuid,
            Name = "文房具"
        };

        var domain = _adapter.ToDomain(source);

        Assert.AreEqual(uuid, domain.Id);
        Assert.AreEqual("文房具", domain.Name);
    }

    [TestMethod(DisplayName = "ドメインエンティティからEFエンティティへ変換できる")]
    public void ToSource_ValidDomainEntity_ReturnsEntity()
    {
        var uuid = Guid.NewGuid();
        var domain = new DomainProductCategory(uuid, "文房具");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(uuid, source.CategoryUuid);
        Assert.AreEqual("文房具", source.Name);
    }

    [TestMethod(DisplayName = "ToSourceではDB採番の主キーを設定しない")]
    public void ToSource_DoesNotSetDatabaseGeneratedId()
    {
        var domain = new DomainProductCategory(Guid.NewGuid(), "文房具");

        var source = _adapter.ToSource(domain);

        Assert.AreEqual(0, source.Id);
    }

    [TestMethod(DisplayName = "変換を往復しても識別IDと名前が保持される")]
    public void ToDomain_AfterToSource_PreservesValues()
    {
        var uuid = Guid.NewGuid();
        var original = new DomainProductCategory(uuid, "文房具");

        var restored = _adapter.ToDomain(_adapter.ToSource(original));

        Assert.AreEqual(original, restored);
        Assert.AreEqual(original.Name, restored.Name);
    }
}