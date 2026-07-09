using System.Data.Common;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Exceptions;
using Backend.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Repositories;

/// <summary>
/// 社員アカウントのリポジトリ実装
/// </summary>
public class EmployeeAccountRepository : IEmployeeAccountRepository
{
    private readonly AppDbContext _context;
    private readonly EmployeeAccountAdapter _adapter;
    private readonly EmployeeAccountFactory _factory;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="adapter">社員アカウントのアダプタ</param>
    /// <param name="factory">アカウント集約を組み立てるファクトリ</param>
    public EmployeeAccountRepository(
        AppDbContext context,
        EmployeeAccountAdapter adapter,
        EmployeeAccountFactory factory)
    {
        _context = context;
        _adapter = adapter;
        _factory = factory;
    }

    /// <summary>
    /// アカウント名を指定して社員アカウントを取得
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>該当する社員アカウント。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<EmployeeAccount?> FindByAccountNameAsync(string accountName)
    {
        try
        {
            var entity = await _context.EmployeeAccounts
                .AsNoTracking()
                .Include(e => e.Employee)
                .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(e => e.Name == accountName);

            return entity is null ? null : _factory.Create(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("社員アカウントの取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 指定したアカウント名が既に登録されているかを確認
    /// </summary>
    /// <param name="accountName">アカウント名</param>
    /// <returns>登録済みの場合はtrue</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<bool> ExistsByAccountNameAsync(string accountName)
    {
        try
        {
            return await _context.EmployeeAccounts
                .AsNoTracking()
                .AnyAsync(e => e.Name == accountName);
        }
        catch (DbException ex)
        {
            throw new InternalException("アカウント名の重複確認に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 社員アカウントを新規登録
    /// </summary>
    /// <param name="account">登録する社員アカウント</param>
    /// <exception cref="DomainException">紐づく社員が存在しない場合</exception>
    /// <exception cref="InternalException">データベースへの登録に失敗した場合</exception>
    public async Task AddAsync(EmployeeAccount account)
    {
        try
        {
            // 社員の外部キーを uuid から解決する
            var employeeEntity = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeUuid == account.Employee.Id);

            if (employeeEntity is null)
            {
                throw new DomainException("指定された社員が存在しません。");
            }

            var entity = _adapter.ToSource(account);
            entity.EmployeeId = employeeEntity.Id;

            _context.EmployeeAccounts.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("社員アカウントの登録に失敗しました。", ex);
        }
        catch (DbException ex)
        {
            throw new InternalException("社員情報の取得に失敗しました。", ex);
        }
    }
}