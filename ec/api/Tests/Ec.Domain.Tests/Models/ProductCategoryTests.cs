using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class ProductCategoryTests
{
    /// <summary>
    /// テスト用の商品カテゴリを生成する（各項目は任意で上書き可能）
    /// </summary>
    private static ProductCategory CreateCategory(Guid? id = null, string name = "文房具")
        => new(id ?? Guid.NewGuid(), name);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();

        var category = CreateCategory(id: id, name: "文房具");

        Assert.AreEqual(id, category.Id);
        Assert.AreEqual("文房具", category.Name);
    }

    [TestMethod(DisplayName = "カテゴリ名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCategory(name: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCategory(name: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCategory(name: "   "));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCategory(id: Guid.Empty));
    }
}