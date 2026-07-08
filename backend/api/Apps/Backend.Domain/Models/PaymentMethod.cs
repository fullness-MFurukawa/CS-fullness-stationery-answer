using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;
/// <summary>
/// 支払い方法を表すドメインエンティティ
/// </summary>
public sealed class PaymentMethod : Entity<int>
{
    /// <summary>
    /// 支払い方法名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">支払い方法ID</param>
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