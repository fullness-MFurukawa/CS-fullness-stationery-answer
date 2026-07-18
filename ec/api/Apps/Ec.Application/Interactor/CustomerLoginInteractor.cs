using System.Security.Claims;
using Ec.Application.Exceptions;
using Ec.Application.Interfaces;
using Ec.Application.Params;
using Ec.Application.Results;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC002:顧客ログインのユースケース実装
/// </summary>
public class CustomerLoginInteractor : ICustomerLoginUsecase
{
    /// <summary>
    /// 顧客であることを表すロールの値
    /// </summary>
    /// <remarks>
    /// トークンにロールを含めることで、顧客のトークンで管理サービスのAPIを
    /// 呼び出せないようにする。EC側と管理側は署名鍵を分けているため、
    /// 本来は鍵の違いだけで区別できるが、ロールを入れて防御を多重にしておく。
    /// </remarks>
    private const string CustomerRole = "customer";

    /// <summary>
    /// 認証失敗時の共通メッセージ（原因を特定させないため一種類に統一する）
    /// </summary>
    private const string AuthenticationErrorMessage = "メールアドレスまたはパスワードが正しくありません。";

    /// <summary>
    /// アカウントが存在しない場合に照合処理の時間を揃えるためのダミーハッシュ
    /// </summary>
    private const string DummyPasswordHash =
        "AQAAAAEAAYagAAAAEP7skW1gcyhyAYGBae+WVI0u5as7DgmHJQc+ef75AOg8H7whgZ8XxbU4/yk3yRKpAQ==";

    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerRepository">顧客のリポジトリ</param>
    /// <param name="passwordHasher">パスワードのハッシュ化と照合</param>
    /// <param name="accessTokenGenerator">アクセストークンの生成</param>
    public CustomerLoginInteractor(
        ICustomerRepository customerRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _customerRepository = customerRepository;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
    }

    /// <summary>
    /// メールアドレスとパスワードで認証し、認証済み顧客とアクセストークンを返す
    /// </summary>
    /// <param name="param">顧客ログインの入力値</param>
    /// <returns>顧客ログインの実行結果</returns>
    /// <exception cref="AuthenticationFailedException">顧客が存在しない、またはパスワードが一致しない場合</exception>
    public async Task<CustomerLoginResult> ExecuteAsync(CustomerLoginParam param)
    {
        var customer = await _customerRepository.FindByMailAddressAsync(param.MailAddress);
        if (customer is null)
        {
            // 顧客の存在有無を応答時間から推測されないよう、ダミーで照合してから失敗させる
            _passwordHasher.VerifyPassword(DummyPasswordHash, param.Password);
            throw new AuthenticationFailedException(AuthenticationErrorMessage);
        }
        if (!_passwordHasher.VerifyPassword(customer.Password, param.Password))
        {
            throw new AuthenticationFailedException(AuthenticationErrorMessage);
        }

        var token = _accessTokenGenerator.Generate(CreateClaims(customer));
        return new CustomerLoginResult(customer, token);
    }

    /// <summary>
    /// 認証済みの顧客からトークンに含めるクレームを組み立てる
    /// </summary>
    /// <param name="customer">認証に成功した顧客</param>
    /// <returns>クレームの一覧</returns>
    /// <remarks>
    /// JWTのペイロードは署名されるだけで暗号化されないため、機密情報は含めない。
    /// </remarks>
    private static IEnumerable<Claim> CreateClaims(Customer customer)
        =>
        [
            new Claim("sub", customer.Id.ToString()),
            new Claim("name", customer.Name),
            new Claim("role", CustomerRole)
        ];
}