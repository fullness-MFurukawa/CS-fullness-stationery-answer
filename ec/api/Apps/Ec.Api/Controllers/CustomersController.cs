using Ec.Api.Adapters;
using Ec.Api.ViewModels.Requests;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 顧客アカウントのAPI
/// </summary>
[ApiController]
[Route("api/ec/customers")]
[Produces("application/json")]
[Tags("顧客")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRegisterUsecase _customerRegisterUsecase;
    private readonly CustomerRegisterRequestAdapter _requestAdapter;
    private readonly CustomerResponseAdapter _responseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerRegisterUsecase">顧客アカウント登録のユースケース</param>
    /// <param name="requestAdapter">登録のリクエストアダプタ</param>
    /// <param name="responseAdapter">顧客のレスポンスアダプタ</param>
    public CustomersController(
        ICustomerRegisterUsecase customerRegisterUsecase,
        CustomerRegisterRequestAdapter requestAdapter,
        CustomerResponseAdapter responseAdapter)
    {
        _customerRegisterUsecase = customerRegisterUsecase;
        _requestAdapter = requestAdapter;
        _responseAdapter = responseAdapter;
    }

    /// <summary>
    /// UC001:顧客アカウントを登録する
    /// </summary>
    /// <param name="request">顧客アカウント登録のリクエスト</param>
    /// <returns>登録された顧客</returns>
    /// <remarks>
    /// 未ログインでも利用できる。
    /// メールアドレスまたはアカウント名が既に登録されている場合は409を返す。
    /// </remarks>
    [EndpointSummary("顧客アカウント登録")]
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> RegisterAsync(CustomerRegisterRequest request)
    {
        var param = _requestAdapter.ToDomain(request);
        var customer = await _customerRegisterUsecase.ExecuteAsync(param);
        var response = _responseAdapter.ToSource(customer);
        return CreatedAtAction(null, response);
    }
}