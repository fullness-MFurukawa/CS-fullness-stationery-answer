using Ec.Application.Params;
using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC001:顧客アカウント登録のユースケース
/// </summary>
public interface ICustomerRegisterUsecase
{
    /// <summary>
    /// 顧客アカウントを登録する
    /// </summary>
    /// <param name="param">顧客アカウント登録の入力値</param>
    /// <returns>登録された顧客</returns>
    Task<Customer> ExecuteAsync(CustomerRegisterParam param);
}