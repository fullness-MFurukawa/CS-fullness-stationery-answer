using Ec.Application.Params;
using Ec.Application.Results;
namespace Ec.Application.Usecases;

/// <summary>
/// UC002:顧客ログインのユースケース
/// </summary>
public interface ICustomerLoginUsecase
{
    /// <summary>
    /// メールアドレスとパスワードで認証し、認証済み顧客とアクセストークンを返す
    /// </summary>
    /// <param name="param">顧客ログインの入力値</param>
    /// <returns>顧客ログインの実行結果</returns>
    Task<CustomerLoginResult> ExecuteAsync(CustomerLoginParam param);
}