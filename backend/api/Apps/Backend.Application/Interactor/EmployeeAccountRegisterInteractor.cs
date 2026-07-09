using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC009:担当者アカウント登録のユースケース実装
/// </summary>
public class EmployeeAccountRegisterInteractor : IEmployeeAccountRegisterUsecase
{
    private readonly IEmployeeAccountRepository _employeeAccountRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントのリポジトリ</param>
    /// <param name="employeeRepository">社員のリポジトリ</param>
    /// <param name="passwordHasher">パスワードのハッシュ化と照合</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public EmployeeAccountRegisterInteractor(
        IEmployeeAccountRepository employeeAccountRepository,
        IEmployeeRepository employeeRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _employeeAccountRepository = employeeAccountRepository;
        _employeeRepository = employeeRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定した社員に対して新しいアカウントを登録する
    /// </summary>
    /// <param name="param">担当者アカウント登録の入力値</param>
    /// <returns>登録された社員アカウント</returns>
    /// <exception cref="ExistsException">アカウント名が既に使用されている場合</exception>
    /// <exception cref="NotFoundException">指定された社員が存在しない場合</exception>
    public async Task<EmployeeAccount> ExecuteAsync(EmployeeAccountRegisterParam param)
    {
        // トランザクション境界を制御して社員アカウントを登録する
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // アカウント名の重複チェック
            if (await _employeeAccountRepository.ExistsByAccountNameAsync(param.AccountName))
            {
                throw new ExistsException("このアカウント名は既に使用されています。");
            }

            // アカウントを紐づける社員を取得する
            var employee = await _employeeRepository.FindByIdAsync(param.EmployeeId)
                ?? throw new NotFoundException("指定された社員が存在しません。");

            // パスワードをハッシュ化してからアカウント集約を構築する
            var hashedPassword = _passwordHasher.HashPassword(param.Password);
            var account = new EmployeeAccount(Guid.NewGuid(), param.AccountName, hashedPassword, employee);

            await _employeeAccountRepository.AddAsync(account);

            return account;
        });
    }
}