using System.Data.Common;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
namespace Ec.Infrastructure.Repositories;

/// <summary>
/// 顧客（EC利用者ログイン用）のリポジトリ実装
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;
    private readonly CustomerAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">データベースコンテキスト</param>
    /// <param name="adapter">顧客のアダプタ</param>
    public CustomerRepository(AppDbContext context, CustomerAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// メールアドレスを指定して顧客を取得
    /// </summary>
    /// <param name="mailAddress">メールアドレス</param>
    /// <returns>該当する顧客。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Customer?> FindByMailAddressAsync(string mailAddress)
    {
        try
        {
            var entity = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.MailAddress == mailAddress);
            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("顧客情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 顧客識別IDを指定して顧客を取得
    /// </summary>
    /// <param name="id">顧客識別ID(uuid)</param>
    /// <returns>該当する顧客。存在しない場合はnull</returns>
    /// <exception cref="InternalException">データベースからの取得に失敗した場合</exception>
    public async Task<Customer?> FindByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.CustomerUuid == id);
            return entity is null ? null : _adapter.ToDomain(entity);
        }
        catch (DbException ex)
        {
            throw new InternalException("顧客情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 指定したメールアドレスが既に登録されているかを確認
    /// </summary>
    /// <param name="mailAddress">メールアドレス</param>
    /// <returns>登録済みの場合はtrue</returns>
    /// <exception cref="InternalException">データベースアクセスに失敗した場合</exception>
    public async Task<bool> ExistsByMailAddressAsync(string mailAddress)
    {
        try
        {
            return await _context.Customers.AnyAsync(e => e.MailAddress == mailAddress);
        }
        catch (DbException ex)
        {
            throw new InternalException("顧客情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 指定したアカウント名が既に登録されているかを確認
    /// </summary>
    /// <param name="username">アカウント名</param>
    /// <returns>登録済みの場合はtrue</returns>
    /// <exception cref="InternalException">データベースアクセスに失敗した場合</exception>
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        try
        {
            return await _context.Customers.AnyAsync(e => e.Username == username);
        }
        catch (DbException ex)
        {
            throw new InternalException("顧客情報の取得に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 顧客を新規登録
    /// </summary>
    /// <param name="customer">登録する顧客</param>
    /// <exception cref="InternalException">データベースへの登録に失敗した場合</exception>
    public async Task AddAsync(Customer customer)
    {
        try
        {
            var entity = _adapter.ToSource(customer);
            _context.Customers.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InternalException("顧客の登録に失敗しました。", ex);
        }
    }
}