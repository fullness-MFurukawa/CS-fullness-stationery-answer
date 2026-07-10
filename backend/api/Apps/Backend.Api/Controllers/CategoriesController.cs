using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 商品カテゴリのAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategorySearchUsecase _categorySearchUsecase;
    private readonly ICategoryRegisterUsecase _categoryRegisterUsecase;
    private readonly CategoryResponseAdapter _categoryResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categorySearchUsecase">商品カテゴリ検索のユースケース</param>
    /// <param name="categoryRegisterUsecase">商品カテゴリ登録のユースケース</param>
    /// <param name="categoryResponseAdapter">商品カテゴリのレスポンスアダプタ</param>
    public CategoriesController(
        ICategorySearchUsecase categorySearchUsecase,
        ICategoryRegisterUsecase categoryRegisterUsecase,
        CategoryResponseAdapter categoryResponseAdapter)
    {
        _categorySearchUsecase = categorySearchUsecase;
        _categoryRegisterUsecase = categoryRegisterUsecase;
        _categoryResponseAdapter = categoryResponseAdapter;
    }

    /// <summary>
    /// 商品カテゴリの一覧を取得する（補助ユースケース）
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    /// <remarks>
    /// 商品登録・商品修正・商品検索の選択肢として使用する。
    /// 該当が0件の場合は空の配列を返す。
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> SearchAsync()
    {
        var categories = await _categorySearchUsecase.ExecuteAsync();

        var response = categories
            .Select(_categoryResponseAdapter.ToSource)
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// UC014:商品カテゴリを登録する
    /// </summary>
    /// <param name="request">商品カテゴリ登録のリクエスト</param>
    /// <returns>登録された商品カテゴリ</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoryResponse>> RegisterAsync(CategoryRegisterRequest request)
    {
        var category = await _categoryRegisterUsecase.ExecuteAsync(request.Name);

        var response = _categoryResponseAdapter.ToSource(category);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}