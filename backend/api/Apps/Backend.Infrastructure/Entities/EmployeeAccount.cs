using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 社員アカウントテーブル(employee_account)に対応するEF Coreエンティティ
/// </summary>
[Table("employee_account")]
public class EmployeeAccount
{
    /// <summary>
    /// アカウントID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// アカウント識別ID
    /// </summary>
    [Column("account_uuid")]
    public Guid AccountUuid { get; set; }

    /// <summary>
    /// アカウント名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// パスワード(ハッシュ値)
    /// </summary>
    [Column("password")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 社員ID(外部キー)
    /// </summary>
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    /// <summary>
    /// 紐づく社員（ナビゲーションプロパティ）
    /// </summary>
    public Employee Employee { get; set; } = null!;
}