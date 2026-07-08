using Backend.Domain.Exceptions;
using Backend.Domain.Models;

namespace Backend.Domain.Tests.Models;

[TestClass]
[TestCategory("Backend.Domain.Models")]
public class EntityTests
{
    /// <summary>
    /// テスト用の具象エンティティ
    /// </summary>
    private sealed class SampleEntity : Entity<Guid>
    {
        public SampleEntity(Guid id) : base(id) { }
    }

    /// <summary>
    /// 型違いを検証するための別の具象エンティティ
    /// </summary>
    private sealed class OtherEntity : Entity<Guid>
    {
        public OtherEntity(Guid id) : base(id) { }
    }

    [TestMethod(DisplayName ="同じ識別子を持つ同一型のエンティティは等価になる")]
    public void Equals_SameTypeAndSameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new SampleEntity(id);

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod(DisplayName = "異なる識別子のエンティティは非等価になる")]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new SampleEntity(Guid.NewGuid());
        var b = new SampleEntity(Guid.NewGuid());

        Assert.AreNotEqual(a, b);
        Assert.IsTrue(a != b);
    }

    [TestMethod(DisplayName = "識別子が同じでも型が異なれば非等価になる")]
    public void Equals_DifferentTypeButSameId_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var a = new SampleEntity(id);
        var b = new OtherEntity(id);

        Assert.AreNotEqual<object>(a, b);
    }

    [TestMethod(DisplayName = "空GUIDの識別子はDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => new SampleEntity(Guid.Empty));
    }
}