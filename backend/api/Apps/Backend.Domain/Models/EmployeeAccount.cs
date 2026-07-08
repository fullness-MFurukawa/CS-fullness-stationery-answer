using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;

/// <summary>
/// 社員アカウント(管理者ログイン用)を表すドメインエンティティ
/// </summary>
public sealed class EmployeeAccount : Entity<Guid>
{
    /// <summary>
    /// アカウント名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// パスワード（ハッシュ値）
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// 紐づく社員
    /// </summary>
    public Employee Employee { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">アカウント識別ID(uuid)</param>
    /// <param name="name">アカウント名</param>
    /// <param name="password">パスワード（ハッシュ値）</param>
    /// <param name="employee">紐づく社員</param>
    /// <exception cref="DomainException">アカウント名・パスワードが未指定、または社員が未指定の場合</exception>
    public EmployeeAccount(Guid id, string name, string password, Employee employee)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("アカウント名が指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException("パスワードが指定されていません。");
        }
        if (employee is null)
        {
            throw new DomainException("社員が指定されていません。");
        }
        Name = name;
        Password = password;
        Employee = employee;
    }
}