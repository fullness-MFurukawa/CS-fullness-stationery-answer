using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class ProductCategoryTests
{
    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();

        var category = new ProductCategory(id, "文房具");

        Assert.AreEqual(id, category.Id);
        Assert.AreEqual("文房具", category.Name);
    }

    [TestMethod(DisplayName = "商品カテゴリ名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new ProductCategory(Guid.NewGuid(), null!));
        Assert.ThrowsExactly<DomainException>(() => new ProductCategory(Guid.NewGuid(), ""));
        Assert.ThrowsExactly<DomainException>(() => new ProductCategory(Guid.NewGuid(), "   "));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new ProductCategory(Guid.Empty, "文房具"));
    }
}