using Backend.Domain.Exceptions;

namespace Backend.Domain.Models;

/// <summary>
/// 顧客（アカウント）を表すドメインエンティティ
/// </summary>
public sealed class Customer : Entity<Guid>
{
    /// <summary>
    /// 顧客名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 顧客名カナ
    /// </summary>
    public string? NameKana { get; }

    /// <summary>
    /// 住所1
    /// </summary>
    public string Address1 { get; }

    /// <summary>
    /// 住所2
    /// </summary>
    public string? Address2 { get; }

    /// <summary>
    /// 電話番号
    /// </summary>
    public string PhoneNumber { get; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string MailAddress { get; }

    /// <summary>
    /// アカウント名
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// パスワード（ハッシュ値）
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// 登録日
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">顧客識別ID(uuid)</param>
    /// <param name="name">顧客名</param>
    /// <param name="nameKana">顧客名カナ</param>
    /// <param name="address1">住所1</param>
    /// <param name="address2">住所2</param>
    /// <param name="phoneNumber">電話番号</param>
    /// <param name="mailAddress">メールアドレス</param>
    /// <param name="username">アカウント名</param>
    /// <param name="password">パスワード（ハッシュ値）</param>
    /// <param name="createdAt">登録日</param>
    /// <exception cref="DomainException">必須項目（顧客名・住所1・電話番号・メールアドレス・アカウント名・パスワード）が未指定の場合</exception>
    public Customer(
        Guid id,
        string name,
        string? nameKana,
        string address1,
        string? address2,
        string phoneNumber,
        string mailAddress,
        string username,
        string password,
        DateTime createdAt) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("顧客名が指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(address1))
        {
            throw new DomainException("住所1が指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new DomainException("電話番号が指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(mailAddress))
        {
            throw new DomainException("メールアドレスが指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainException("アカウント名が指定されていません。");
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException("パスワードが指定されていません。");
        }
        Name = name;
        NameKana = nameKana;
        Address1 = address1;
        Address2 = address2;
        PhoneNumber = phoneNumber;
        MailAddress = mailAddress;
        Username = username;
        Password = password;
        CreatedAt = createdAt;
    }
}