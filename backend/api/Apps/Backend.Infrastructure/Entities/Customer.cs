using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 顧客(アカウント)テーブル(customer)に対応するEF Coreエンティティ
/// </summary>
[Table("customer")]
public class Customer
{
    /// <summary>
    /// 顧客ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 顧客識別ID
    /// </summary>
    [Column("customer_uuid")]
    public Guid CustomerUuid { get; set; }

    /// <summary>
    /// 顧客名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 顧客名カナ
    /// </summary>
    [Column("name_kana")]
    public string? NameKana { get; set; }

    /// <summary>
    /// 住所1
    /// </summary>
    [Column("address1")]
    public string Address1 { get; set; } = null!;

    /// <summary>
    /// 住所2
    /// </summary>
    [Column("address2")]
    public string? Address2 { get; set; }

    /// <summary>
    /// 電話番号
    /// </summary>
    [Column("phone_number")]
    public string PhoneNumber { get; set; } = null!;

    /// <summary>
    /// メールアドレス
    /// </summary>
    [Column("mail_address")]
    public string MailAddress { get; set; } = null!;

    /// <summary>
    /// アカウント名
    /// </summary>
    [Column("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// パスワード(ハッシュ値)
    /// </summary>
    [Column("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 登録日
    /// </summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// この顧客の注文の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}