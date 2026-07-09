using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// アカウント未登録の社員一覧取得のユースケース実装
/// </summary>
public class EmployeeWithoutAccountSearchInteractor : IEmployeeWithoutAccountSearchUsecase
{
    private readonly IEmployeeRepository _employeeRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeRepository">社員のリポジトリ</param>
    public EmployeeWithoutAccountSearchInteractor(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// アカウントが未登録の社員をすべて取得する
    /// </summary>
    /// <returns>アカウント未登録の社員一覧</returns>
    public async Task<IReadOnlyList<Employee>> ExecuteAsync()
    {
        return await _employeeRepository.FindWithoutAccountAsync();
    }
}