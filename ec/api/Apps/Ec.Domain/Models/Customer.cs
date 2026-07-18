using System.Text.RegularExpressions;
using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 顧客（アカウント）を表すドメインエンティティ
/// </summary>
/// <remarks>
/// 管理サービス側にも同名のエンティティが存在するが、役割が異なる。
/// 管理側は購入履歴に顧客名を表示するための参照用であるのに対し、
/// EC側は顧客自身が登録する主体であるため、登録時の業務制約を持つ。
/// </remarks>
public sealed class Customer : Entity<Guid>
{
    /// <summary>
    /// メールアドレスの形式を検証する正規表現
    /// </summary>
    /// <remarks>
    /// RFC に完全準拠した検証は現実的でないため、
    /// 「@ の前後に文字があり、ドメイン部にドットを含む」程度の確認に留める。
    /// 到達性の確認は本来メールの送信によって行うものであり、
    /// 正規表現を厳密にしても得られるものは少ない。
    /// </remarks>
    private static readonly Regex MailAddressPattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    /// <summary>
    /// 顧客名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 顧客名カナ
    /// </summary>
    /// <remarks>
    /// データベースの定義では NULL を許容しているが、
    /// 画面仕様(FP003)では必須項目のため、ドメインでも必須として扱う。
    /// </remarks>
    public string NameKana { get; }

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
    /// ログイン時の認証に使用する（UC002）
    /// </summary>
    public string MailAddress { get; }

    /// <summary>
    /// アカウント名
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// パスワード（ハッシュ値）
    /// </summary>
    /// <remarks>
    /// ハッシュ化はアプリケーション層の責務であり、
    /// このエンティティはハッシュ済みの文字列を受け取るだけとする。
    /// ハッシュのアルゴリズムは業務ルールではなく実装の詳細であるため、
    /// ドメインが知る必要はない。
    /// </remarks>
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
    /// <exception cref="DomainException">必須項目が未指定の場合、またはメールアドレスの形式が不正な場合</exception>
    public Customer(
        Guid id,
        string name,
        string nameKana,
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
        if (string.IsNullOrWhiteSpace(nameKana))
        {
            throw new DomainException("顧客名カナが指定されていません。");
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
        if (!MailAddressPattern.IsMatch(mailAddress))
        {
            throw new DomainException("メールアドレスの形式が正しくありません。");
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