using System.ComponentModel.DataAnnotations;
namespace Ec.Api.ViewModels.Requests;

/// <summary>
/// UC005:購入確定のリクエスト
/// </summary>
/// <remarks>
/// 顧客識別IDはリクエストに含めない。詐称を防ぐため、認証済みのトークンから取得する。
/// </remarks>
/// <param name="PaymentMethodId">支払い方法の識別ID</param>
/// <param name="Items">注文する商品と数量の組</param>
public sealed record OrderCreateRequest(
    [Required(ErrorMessage = "支払い方法を選択してください")]
    int? PaymentMethodId,

    [Required(ErrorMessage = "注文する商品を指定してください")]
    [MinLength(1, ErrorMessage = "注文する商品を1件以上指定してください")]
    IReadOnlyList<OrderItemRequest> Items);

/// <summary>
/// 注文する商品1件分のリクエスト
/// </summary>
/// <param name="ProductId">商品の識別ID(uuid)</param>
/// <param name="Count">注文数量</param>
public sealed record OrderItemRequest(
    [Required(ErrorMessage = "商品を指定してください")]
    Guid? ProductId,

    [Range(1, int.MaxValue, ErrorMessage = "数量は1以上を指定してください")]
    int Count);