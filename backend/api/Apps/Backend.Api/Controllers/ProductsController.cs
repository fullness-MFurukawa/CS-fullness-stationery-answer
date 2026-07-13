using Backend.Api.Adapters;
using Backend.Api.ViewModels.Requests;
using Backend.Api.ViewModels.Responses;
using Backend.Application.Usecases;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

/// <summary>
/// 商品のAPI
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductSearchUsecase _productSearchUsecase;
    private readonly IProductRegisterUsecase _productRegisterUsecase;
    private readonly IProductUpdateUsecase _productUpdateUsecase;
    private readonly IProductDeleteUsecase _productDeleteUsecase;
    private readonly ProductRegisterRequestAdapter _productRegisterRequestAdapter;
    private readonly ProductUpdateRequestAdapter _productUpdateRequestAdapter;
    private readonly ProductResponseAdapter _productResponseAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="productSearchUsecase">商品検索のユースケース</param>
    /// <param name="productRegisterUsecase">新商品登録のユースケース</param>
    /// <param name="productUpdateUsecase">商品修正のユースケース</param>
    /// <param name="productDeleteUsecase">商品削除のユースケース</param>
    /// <param name="productRegisterRequestAdapter">新商品登録のリクエストアダプタ</param>
    /// <param name="productUpdateRequestAdapter">商品修正のリクエストアダプタ</param>
    /// <param name="productResponseAdapter">商品のレスポンスアダプタ</param>
    public ProductsController(
        IProductSearchUsecase productSearchUsecase,
        IProductRegisterUsecase productRegisterUsecase,
        IProductUpdateUsecase productUpdateUsecase,
        IProductDeleteUsecase productDeleteUsecase,
        ProductRegisterRequestAdapter productRegisterRequestAdapter,
        ProductUpdateRequestAdapter productUpdateRequestAdapter,
        ProductResponseAdapter productResponseAdapter)
    {
        _productSearchUsecase = productSearchUsecase;
        _productRegisterUsecase = productRegisterUsecase;
        _productUpdateUsecase = productUpdateUsecase;
        _productDeleteUsecase = productDeleteUsecase;
        _productRegisterRequestAdapter = productRegisterRequestAdapter;
        _productUpdateRequestAdapter = productUpdateRequestAdapter;
        _productResponseAdapter = productResponseAdapter;
    }

    /// <summary>
    /// UC011:商品を検索する
    /// </summary>
    /// <param name="categoryId">商品カテゴリ識別ID(uuid)。指定しない場合は全件取得する</param>
    /// <returns>論理削除を除いた商品の一覧</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> SearchAsync([FromQuery] Guid? categoryId)
    {
        var products = await _productSearchUsecase.ExecuteAsync(categoryId);

        // 該当0件は正常系のため、空配列を返す
        var response = products.Select(_productResponseAdapter.ToSource).ToList();

        return Ok(response);
    }

    /// <summary>
    /// UC010:新しい商品を登録する
    /// </summary>
    /// <param name="request">新商品登録のリクエスト（画像ファイルを含む）</param>
    /// <returns>登録された商品</returns>
    /// <remarks>画像ファイルを含むため multipart/form-data で受け取る。</remarks>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> RegisterAsync([FromForm] ProductRegisterRequest request)
    {
        var param = _productRegisterRequestAdapter.ToDomain(request);
        var product = await _productRegisterUsecase.ExecuteAsync(param);

        var response = _productResponseAdapter.ToSource(product);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// 商品情報を修正する（UC012）
    /// </summary>
    /// <param name="productId">修正対象の商品識別ID(uuid)</param>
    /// <param name="request">商品修正のリクエスト（画像ファイルを含む）</param>
    /// <returns>修正された商品</returns>
    /// <remarks>画像ファイルを含むため multipart/form-data で受け取る。</remarks>
    [HttpPut("{productId:guid}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> UpdateAsync(Guid productId, [FromForm] ProductUpdateRequest request)
    {
        // 対象の識別子はルートパラメータを正とし、リクエストボディの値は使用しない
        var param = _productUpdateRequestAdapter.ToDomain(request with { ProductId = productId });
        var product = await _productUpdateUsecase.ExecuteAsync(param);

        var response = _productResponseAdapter.ToSource(product);

        return Ok(response);
    }

    /// <summary>
    /// 商品を削除する（UC013）
    /// </summary>
    /// <param name="productId">削除対象の商品識別ID(uuid)</param>
    /// <returns>削除された商品</returns>
    /// <remarks>物理削除は行わず、削除フラグを更新する。</remarks>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> DeleteAsync(Guid productId)
    {
        var product = await _productDeleteUsecase.ExecuteAsync(productId);

        var response = _productResponseAdapter.ToSource(product);

        return Ok(response);
    }
}