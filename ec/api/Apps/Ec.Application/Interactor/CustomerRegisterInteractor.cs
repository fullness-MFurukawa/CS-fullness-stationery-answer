using Ec.Application.Exceptions;
using Ec.Application.Interfaces;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC001:顧客アカウント登録のユースケース実装
/// </summary>
public class CustomerRegisterInteractor : ICustomerRegisterUsecase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerRepository">顧客のリポジトリ</param>
    /// <param name="passwordHasher">パスワードのハッシュ化と照合</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public CustomerRegisterInteractor(
        ICustomerRepository customerRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 顧客アカウントを登録する
    /// </summary>
    /// <param name="param">顧客アカウント登録の入力値</param>
    /// <returns>登録された顧客</returns>
    /// <exception cref="ExistsException">メールアドレスまたはアカウント名が既に使用されている場合</exception>
    public async Task<Customer> ExecuteAsync(CustomerRegisterParam param)
    {
        // トランザクション境界を制御して顧客を登録する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // メールアドレスの重複チェック。
            // 画面仕様(FP003)ではメールアドレスとアカウント名で
            // それぞれ別のエラーメッセージを表示するため、確認を分けて行う。
            if (await _customerRepository.ExistsByMailAddressAsync(param.MailAddress))
            {
                throw new ExistsException("このメールアドレスは既に登録されています。");
            }
            // アカウント名の重複チェック
            if (await _customerRepository.ExistsByUsernameAsync(param.Username))
            {
                throw new ExistsException("このアカウント名は既に使用されています。");
            }

            // パスワードをハッシュ化してから顧客集約を構築する。
            // 桁数や文字種の検証はApi層のリクエストDTOで済んでいる前提で、
            // ここではドメインの不変条件（必須・メール形式）のみが働く。
            var hashedPassword = _passwordHasher.HashPassword(param.Password);
            var customer = new Customer(
                Guid.NewGuid(),
                param.Name,
                param.NameKana,
                param.Address1,
                param.Address2,
                param.PhoneNumber,
                param.MailAddress,
                param.Username,
                hashedPassword,
                DateTime.Now);

            await _customerRepository.AddAsync(customer);
            return customer;
        });
    }
}