using Backend.Api.Adapters;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 社員のAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/employees")]
[Produces("application/json")]
[Tags("従業員")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeWithoutAccountSearchUsecase _employeeWithoutAccountSearchUsecase;
    private readonly EmployeeResponseAdapter _employeeResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeWithoutAccountSearchUsecase">アカウント未登録の社員一覧取得のユースケース</param>
    /// <param name="employeeResponseAdapter">社員のレスポンスアダプタ</param>
    public EmployeesController(
        IEmployeeWithoutAccountSearchUsecase employeeWithoutAccountSearchUsecase,
        EmployeeResponseAdapter employeeResponseAdapter)
    {
        _employeeWithoutAccountSearchUsecase = employeeWithoutAccountSearchUsecase;
        _employeeResponseAdapter = employeeResponseAdapter;
    }

    /// <summary>
    /// アカウントが未登録の社員をすべて取得する
    /// </summary>
    /// <returns>アカウント未登録の社員一覧</returns>
    /// <remarks>
    /// 担当者アカウント登録画面のプルダウンの選択肢として使用する。
    /// 全社員にアカウントが存在する場合は空配列を返す。
    /// </remarks>
    [HttpGet("without-account")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<EmployeeResponse>>> SearchWithoutAccountAsync()
    {
        var employees = await _employeeWithoutAccountSearchUsecase.ExecuteAsync();

        // 登録可能な社員が0件でも正常系のため、空配列を返す
        var response = employees.Select(_employeeResponseAdapter.ToSource).ToList();

        return Ok(response);
    }
}