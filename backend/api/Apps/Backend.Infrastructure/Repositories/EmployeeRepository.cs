using System.Data.Common;

using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Backend.Infrastructure.Factories;

using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 社員のリポジトリ実装
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;
    private readonly EmployeeFactory _employeeFactory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="employeeFactory">社員集約を組み立てるファクトリ</param>
    public EmployeeRepository(AppDbContext context, EmployeeFactory employeeFactory)
    {
        _context = context;
        _employeeFactory = employeeFactory;
    }

    /// <summary>
    /// アカウント未登録の社員を取得
    /// </summary>
    /// <returns>アカウントが未作成の社員一覧</returns>
    public async Task<IReadOnlyList<Employee>> FindWithoutAccountAsync()
    {
        try
        {
            var entities = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Account == null)
            .OrderBy(e => e.Id)
            .ToListAsync();

            return entities.Select(_employeeFactory.Create).ToList();

        }
        catch (DbException ex)
        {
            throw new InternalException("社員の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 識別IDを指定して社員を取得
    /// </summary>
    /// <param name="id">社員識別ID(uuid)</param>
    /// <returns>該当する社員。存在しない場合はnull</returns>
    public async Task<Employee?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeUuid == id);
            // 存在しない場合はnullを返す
            return entity is null ? null : _employeeFactory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("社員の取得に失敗しました。", ex);
        }
    }
}