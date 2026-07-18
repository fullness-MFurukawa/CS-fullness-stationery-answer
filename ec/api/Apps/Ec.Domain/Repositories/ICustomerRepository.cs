using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 顧客（EC利用者ログイン用）の永続化を担うリポジトリ
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// メールアドレスを指定して顧客を取得
    /// </summary>
    /// <param name="mailAddress">メールアドレス</param>
    /// <returns>該当する顧客。存在しない場合はnull</returns>
    /// <remarks>ログイン認証（UC002）に用いる。</remarks>
    Task<Customer?> FindByMailAddressAsync(string mailAddress);

    /// <summary>
    /// 顧客識別IDを指定して顧客を取得
    /// </summary>
    /// <param name="id">顧客識別ID(uuid)</param>
    /// <returns>該当する顧客。存在しない場合はnull</returns>
    /// <remarks>
    /// 購入確定（UC005）や購入履歴（UC007）で、
    /// 認証済みのトークンに含まれる顧客識別IDから顧客を特定するために用いる。
    /// </remarks>
    Task<Customer?> FindByIdAsync(Guid id);

    /// <summary>
    /// 指定したメールアドレスが既に登録されているかを確認
    /// </summary>
    /// <param name="mailAddress">メールアドレス</param>
    /// <returns>登録済みの場合はtrue</returns>
    Task<bool> ExistsByMailAddressAsync(string mailAddress);

    /// <summary>
    /// 指定したアカウント名が既に登録されているかを確認
    /// </summary>
    /// <param name="username">アカウント名</param>
    /// <returns>登録済みの場合はtrue</returns>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    /// 顧客を新規登録
    /// </summary>
    /// <param name="customer">登録する顧客</param>
    Task AddAsync(Customer customer);
}