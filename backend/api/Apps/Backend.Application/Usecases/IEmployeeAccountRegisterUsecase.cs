using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC009:担当者アカウント登録のユースケース
/// </summary>
public interface IEmployeeAccountRegisterUsecase
{
    /// <summary>
    /// 指定した社員に対して新しいアカウントを登録する
    /// </summary>
    /// <param name="param">担当者アカウント登録の入力値</param>
    /// <returns>登録された社員アカウント</returns>
    Task<EmployeeAccount> ExecuteAsync(EmployeeAccountRegisterParam param);
}