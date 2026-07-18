using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 商品カテゴリを表すドメインエンティティ
/// </summary>
/// <remarks>
/// EC側では商品の絞り込み（UC003）に使うのみで、登録・更新は行わない。
/// カテゴリの登録は管理サービス側（UC014）の責務である。
/// </remarks>
public sealed class ProductCategory : Entity<Guid>
{
    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">商品カテゴリ識別ID(uuid)</param>
    /// <param name="name">カテゴリ名</param>
    /// <exception cref="DomainException">カテゴリ名が未指定の場合</exception>
    public ProductCategory(Guid id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("カテゴリ名が指定されていません。");
        }
        Name = name;
    }
}