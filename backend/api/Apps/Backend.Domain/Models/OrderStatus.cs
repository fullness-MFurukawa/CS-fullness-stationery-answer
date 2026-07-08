using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;

/// <summary>
/// 注文ステータスを表すドメインエンティティ
/// </summary>
public sealed class OrderStatus : Entity<int>
{
    /// <summary>
    /// 注文ステータス名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">注文ステータスID</param>
    /// <param name="name">注文ステータス名</param>
    /// <exception cref="DomainException">注文ステータス名が未指定の場合</exception>
    public OrderStatus(int id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("注文ステータス名が指定されていません。");
        }
        Name = name;
    }
}