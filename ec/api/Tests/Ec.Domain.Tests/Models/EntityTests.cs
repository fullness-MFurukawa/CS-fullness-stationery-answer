using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
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

    /// <summary>
    /// 採番前の状態を検証するための具象エンティティ（識別子はint）
    /// </summary>
    private sealed class NumberedEntity : Entity<int>
    {
        public NumberedEntity(int id) : base(id) { }

        /// <summary>採番前（識別子未設定）で生成する</summary>
        public NumberedEntity() : base() { }
    }

    [TestMethod(DisplayName = "同じ識別子を持つ同一型のエンティティは等価になる")]
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

    [TestMethod(DisplayName = "識別子を指定して生成すると永続化済みとなる")]
    public void IsPersisted_WithId_ReturnsTrue()
    {
        var entity = new NumberedEntity(1);

        Assert.AreEqual(1, entity.Id);
        Assert.IsTrue(entity.IsPersisted);
    }

    [TestMethod(DisplayName = "採番前で生成すると永続化前となり識別子は既定値になる")]
    public void IsPersisted_WithoutId_ReturnsFalse()
    {
        var entity = new NumberedEntity();

        // 採番前のコンストラクタはEC側で追加したもの。
        // データベースの採番に委ねるエンティティ（注文明細など）のために用意した
        Assert.AreEqual(0, entity.Id);
        Assert.IsFalse(entity.IsPersisted);
    }

    [TestMethod(DisplayName = "採番前のエンティティは識別子が同じ既定値のため等価になる")]
    public void Equals_UnpersistedEntities_AreEqual()
    {
        var a = new NumberedEntity();
        var b = new NumberedEntity();

        // 識別子がどちらも0のため等価と判定される。
        // 採番前のエンティティをコレクションで比較する場合に注意が要る挙動であり、
        // 把握しておくためにテストとして残す
        Assert.AreEqual(a, b);
    }
}