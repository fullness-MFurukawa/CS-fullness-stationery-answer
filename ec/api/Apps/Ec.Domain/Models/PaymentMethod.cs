using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 支払い方法を表すドメインエンティティ
/// </summary>
/// <remarks>
/// 他のエンティティと異なり、識別子はUUIDではなく連番(int)である。
/// payment_methodテーブルがマスタであり、外部へUUIDを公開する必要がないため、
/// データベースの定義に合わせている。
/// UC005の時点では「現金」のみ選択できる。
/// </remarks>
public sealed class PaymentMethod : Entity<int>
{
    /// <summary>
    /// 支払い方法名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">支払い方法識別ID</param>
    /// <param name="name">支払い方法名</param>
    /// <exception cref="DomainException">支払い方法名が未指定の場合</exception>
    public PaymentMethod(int id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("支払い方法名が指定されていません。");
        }
        Name = name;
    }
}