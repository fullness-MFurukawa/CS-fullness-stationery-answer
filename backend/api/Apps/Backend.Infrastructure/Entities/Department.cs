using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Infrastructure.Entities;

/// <summary>
/// 部署テーブル(department)に対応するEF Coreエンティティ
/// </summary>
[Table("department")]
public class Department
{
    /// <summary>
    /// 部署ID
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 部署識別ID
    /// </summary>
    [Column("department_uuid")]
    public Guid DepartmentUuid { get; set; }

    /// <summary>
    /// 部署名
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 所属する社員の一覧（ナビゲーションプロパティ）
    /// </summary>
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}