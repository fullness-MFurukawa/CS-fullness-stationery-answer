using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 商品カテゴリのAPI
/// </summary>
[ApiController]
[Route("api/ec/categories")]
[Produces("application/json")]
[Tags("商品カテゴリ")]
public class CategoriesController : ControllerBase
{
    private readonly ICategorySearchUsecase _categorySearchUsecase;
    private readonly CategoryResponseAdapter _categoryResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categorySearchUsecase">商品カテゴリ検索のユースケース</param>
    /// <param name="categoryResponseAdapter">商品カテゴリのレスポンスアダプタ</param>
    public CategoriesController(
        ICategorySearchUsecase categorySearchUsecase,
        CategoryResponseAdapter categoryResponseAdapter)
    {
        _categorySearchUsecase = categorySearchUsecase;
        _categoryResponseAdapter = categoryResponseAdapter;
    }

    /// <summary>
    /// UC003:商品カテゴリの一覧を取得する
    /// </summary>
    /// <returns>商品カテゴリの一覧</returns>
    /// <remarks>
    /// カテゴリ別商品検索画面（FP006）の絞り込みの選択肢として使用する。
    /// 未ログインでも利用できる。該当が0件の場合は空の配列を返す。
    /// </remarks>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> SearchAsync()
    {
        var categories = await _categorySearchUsecase.ExecuteAsync();
        var response = categories.Select(_categoryResponseAdapter.ToSource).ToList();
        return Ok(response);
    }
}