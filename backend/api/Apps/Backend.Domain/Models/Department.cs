using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;
/// <summary>
/// 部署を表すドメインエンティティ
/// </summary>
public sealed class Department : Entity<Guid>
{
    /// <summary>
    /// 部署名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">部署識別ID(uuid)</param>
    /// <param name="name">部署名</param>
    /// <exception cref="DomainException">部署名が未指定の場合</exception>
    public Department(Guid id, string name): base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("部署名が指定されていません。");
        }
        Name = name;
    }
}