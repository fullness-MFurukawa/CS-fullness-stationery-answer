using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// アカウント未登録の社員一覧取得のユースケース
/// 担当者アカウント登録画面で社員の選択肢として使用する
/// </summary>
public interface IEmployeeWithoutAccountSearchUsecase
{
    /// <summary>
    /// アカウントが未登録の社員をすべて取得する
    /// </summary>
    /// <returns>アカウント未登録の社員一覧</returns>
    Task<IReadOnlyList<Employee>> ExecuteAsync();
}