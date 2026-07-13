using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 担当者アカウントのAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/employee-accounts")]
[Produces("application/json")]
[Tags("従業員アカウント")]
public class EmployeeAccountsController : ControllerBase
{
    private readonly IEmployeeAccountRegisterUsecase _employeeAccountRegisterUsecase;
    private readonly EmployeeAccountRegisterRequestAdapter _employeeAccountRegisterRequestAdapter;
    private readonly EmployeeAccountResponseAdapter _employeeAccountResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="employeeAccountRegisterUsecase">担当者アカウント登録のユースケース</param>
    /// <param name="employeeAccountRegisterRequestAdapter">担当者アカウント登録のリクエストアダプタ</param>
    /// <param name="employeeAccountResponseAdapter">社員アカウントのレスポンスアダプタ</param>
    public EmployeeAccountsController(
        IEmployeeAccountRegisterUsecase employeeAccountRegisterUsecase,
        EmployeeAccountRegisterRequestAdapter employeeAccountRegisterRequestAdapter,
        EmployeeAccountResponseAdapter employeeAccountResponseAdapter)
    {
        _employeeAccountRegisterUsecase = employeeAccountRegisterUsecase;
        _employeeAccountRegisterRequestAdapter = employeeAccountRegisterRequestAdapter;
        _employeeAccountResponseAdapter = employeeAccountResponseAdapter;
    }

    /// <summary>
    /// UC009:担当者アカウントを登録する
    /// </summary>
    /// <param name="request">担当者アカウント登録のリクエスト</param>
    /// <returns>登録された担当者アカウント</returns>
    /// <remarks>
    /// アカウント名が既に使用されている場合は409を返す。
    /// 指定した社員が存在しない場合は404を返す。
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeAccountResponse>> RegisterAsync(
        EmployeeAccountRegisterRequest request)
    {
        var param = _employeeAccountRegisterRequestAdapter.ToDomain(request);
        var account = await _employeeAccountRegisterUsecase.ExecuteAsync(param);

        var response = _employeeAccountResponseAdapter.ToSource(account);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}