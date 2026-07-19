using Ec.Api.Adapters;
using Ec.Api.ViewModels.Responses;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Ec.Api.Controllers;

/// <summary>
/// 商品のAPI
/// </summary>
[ApiController]
[Route("api/ec/products")]
[Produces("application/json")]
[Tags("商品")]
public class ProductsController : ControllerBase
{
    private readonly IProductSearchUsecase _productSearchUsecase;
    private readonly IProductDetailUsecase _productDetailUsecase;
    private readonly ProductResponseAdapter _productResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productSearchUsecase">商品検索のユースケース</param>
    /// <param name="productDetailUsecase">商品詳細取得のユースケース</param>
    /// <param name="productResponseAdapter">商品のレスポンスアダプタ</param>
    public ProductsController(
        IProductSearchUsecase productSearchUsecase,
        IProductDetailUsecase productDetailUsecase,
        ProductResponseAdapter productResponseAdapter)
    {
        _productSearchUsecase = productSearchUsecase;
        _productDetailUsecase = productDetailUsecase;
        _productResponseAdapter = productResponseAdapter;
    }

    /// <summary>
    /// UC003:商品を検索する
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)。指定しない場合は全件取得する</param>
    /// <returns>論理削除を除いた商品の一覧</returns>
    /// <remarks>未ログインでも利用できる。該当0件は正常系として空配列を返す。</remarks>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> SearchAsync([FromQuery] Guid? categoryId)
    {
        var products = await _productSearchUsecase.ExecuteAsync(new ProductSearchParam(categoryId));
        var response = products.Select(_productResponseAdapter.ToSource).ToList();
        return Ok(response);
    }

    /// <summary>
    /// UC004:商品の詳細を取得する
    /// </summary>
    /// <param name="productId">商品識別ID(uuid)</param>
    /// <returns>該当する商品</returns>
    /// <remarks>未ログインでも利用できる。存在しない・販売終了の場合は404。</remarks>
    [AllowAnonymous]
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetAsync(Guid productId)
    {
        var product = await _productDetailUsecase.ExecuteAsync(productId);
        var response = _productResponseAdapter.ToSource(product);
        return Ok(response);
    }
}