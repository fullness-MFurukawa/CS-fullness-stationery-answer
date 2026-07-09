using System.Security.Claims;
using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Results;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC017:担当者ログインのユースケース実装
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
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRepository">社員アカウントのリポジトリ</param>
    /// <param name="passwordHasher">パスワードのハッシュ化と照合</param>
    /// <param name="accessTokenGenerator">アクセストークンの生成</param>
    public EmployeeLoginInteractor(
        IEmployeeAccountRepository employeeAccountRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _employeeAccountRepository = employeeAccountRepository;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
    }

    /// <summary>
    /// アカウント名とパスワードで認証し、認証済みアカウントとアクセストークンを返す
    /// </summary>
    /// <param name="param">担当者ログインの入力値</param>
    /// <returns>担当者ログインの実行結果</returns>
    /// <exception cref="AuthenticationFailedException">アカウントが存在しない、またはパスワードが一致しない場合</exception>
    public async Task<EmployeeLoginResult> ExecuteAsync(EmployeeLoginParam param)
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

        // 業務的なクレームを組み立て、トークンの生成はInfrastructureの実装に委ねる
        var token = _accessTokenGenerator.Generate(CreateClaims(account));

        return new EmployeeLoginResult(account, token);
    }

    /// <summary>
    /// 認証済みの社員アカウントからトークンに含めるクレームを組み立てる
    /// </summary>
    /// <param name="account">認証に成功した社員アカウント</param>
    /// <returns>クレームの一覧</returns>
    /// <remarks>
    /// JWTのペイロードは署名されるだけで暗号化されないため、機密情報は含めない。
    /// </remarks>
    private static IEnumerable<Claim> CreateClaims(EmployeeAccount account)
        =>
        [
            new Claim("sub", account.Id.ToString()),
            new Claim("name", account.Name),
            new Claim("employee_name", account.Employee.Name)
        ];
}