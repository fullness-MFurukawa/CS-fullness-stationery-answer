using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC017 担当者ログインのユースケース実装
/// </summary>
public class EmployeeLoginInteractor : IEmployeeLoginUsecase
{
    /// <summary>
    /// 認証失敗時の共通メッセージ（原因を特定させないため一種類に統一する）
    /// </summary>
    private const string AuthenticationErrorMessage = "アカウント名またはパスワードが正しくありません。";

    /// <summary>
    /// アカウントが存在しない場合に照合処理の時間を揃えるためのダミーハッシュ
    /// </summary>
    private const string DummyPasswordHash =
        "AQAAAAEAAYagAAAAEP7skW1gcyhyAYGBae+WVI0u5as7DgmHJQc+ef75AOg8H7whgZ8XxbU4/yk3yRKpAQ==";

    private readonly IEmployeeAccountRepository _employeeAccountRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントのリポジトリ</param>
    /// <param name="passwordHasher">パスワードのハッシュ化と照合</param>
    public EmployeeLoginInteractor(
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHasher passwordHasher)
    {
        _employeeAccountRepository = employeeAccountRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// アカウント名とパスワードで認証し、認証済みの社員アカウントを返す
    /// </summary>
    /// <param name="param">担当者ログインの入力値</param>
    /// <returns>認証に成功した社員アカウント</returns>
    /// <exception cref="AuthenticationFailedException">アカウントが存在しない、またはパスワードが一致しない場合</exception>
    public async Task<EmployeeAccount> ExecuteAsync(EmployeeLoginParam param)
    {
        var account = await _employeeAccountRepository.FindByAccountNameAsync(param.AccountName);

        if (account is null)
        {
            // アカウントの存在有無を応答時間から推測されないよう、ダミーで照合してから失敗させる
            _passwordHasher.VerifyPassword(DummyPasswordHash, param.Password);
            throw new AuthenticationFailedException(AuthenticationErrorMessage);
        }

        if (!_passwordHasher.VerifyPassword(account.Password, param.Password))
        {
            throw new AuthenticationFailedException(AuthenticationErrorMessage);
        }

        return account;
    }
}