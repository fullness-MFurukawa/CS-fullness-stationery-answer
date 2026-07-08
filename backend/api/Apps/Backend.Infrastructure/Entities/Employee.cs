using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 社員テーブル(employee)に対応するEF Coreエンティティ
/// </summary>
[Table("employee")]
public class Employee
{
    /// <summary>
    /// 社員ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 社員識別ID
    /// </summary>
    [Column("employee_uuid")]
    public Guid EmployeeUuid { get; set; }

    /// <summary>
    /// 社員名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 社員名カナ
    /// </summary>
    [Column("name_kana")]
    public string? NameKana { get; set; }

    /// <summary>
    /// 部署ID(外部キー)
    /// </summary>
    [Column("department_id")]
    public int DepartmentId { get; set; }

    /// <summary>
    /// 所属部署（ナビゲーションプロパティ）
    /// </summary>
    public Department Department { get; set; } = null!;

    /// <summary>
    /// この社員に紐づくアカウント（ナビゲーションプロパティ）
    /// </summary>
    public EmployeeAccount? Account { get; set; }
}