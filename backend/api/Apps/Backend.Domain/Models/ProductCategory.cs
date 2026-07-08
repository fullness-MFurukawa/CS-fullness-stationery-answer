using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;

/// <summary>
/// 商品カテゴリを表すドメインエンティティ
/// </summary>
public sealed class ProductCategory : Entity<Guid>
{
    /// <summary>
    /// 商品カテゴリ名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <param name="name">商品カテゴリ名</param>
    /// <exception cref="DomainException">商品カテゴリ名が未指定の場合</exception>
    public ProductCategory(Guid id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("商品カテゴリ名が指定されていません。");
        }
        Name = name;
    }
}