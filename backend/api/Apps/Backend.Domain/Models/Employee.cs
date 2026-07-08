using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;

/// <summary>
/// 社員を表すドメインエンティティ
/// </summary>
public sealed class Employee : Entity<Guid>
{
    /// <summary>
    /// 社員名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 社員名カナ
    /// </summary>
    public string? NameKana { get; }

    /// <summary>
    /// 所属部署
    /// </summary>
    public Department Department { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">社員識別ID(uuid)</param>
    /// <param name="name">社員名</param>
    /// <param name="nameKana">社員名カナ</param>
    /// <param name="department">所属部署</param>
    /// <exception cref="DomainException">社員名が未指定、または所属部署が未指定の場合</exception>
    public Employee(Guid id, string name, string? nameKana, Department department) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("社員名が指定されていません。");
        }
        if (department is null)
        {
            throw new DomainException("所属部署が指定されていません。");
        }
        Name = name;
        NameKana = nameKana;
        Department = department;
    }
}