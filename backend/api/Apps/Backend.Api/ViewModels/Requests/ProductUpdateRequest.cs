using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC012:商品修正のリクエスト
/// </summary>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="ImageUrl">画像URL</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">在庫数</param>
public sealed record ProductUpdateRequest(
    [Required(ErrorMessage = "商品名を入力してください")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "商品名は2～20文字で入力してください")]
    string Name,

    [Required(ErrorMessage = "価格を入力してください")]
    [Range(0, 1_000_000, ErrorMessage = "価格は1,000,000円以下で入力してください")]
    int? Price,

    [StringLength(200, ErrorMessage = "画像URLは200文字以内で入力してください")]
    string? ImageUrl,

    [Required(ErrorMessage = "カテゴリを選択してください")]
    Guid? CategoryId,

    [Required(ErrorMessage = "在庫数を入力してください")]
    [Range(0, 1_000, ErrorMessage = "在庫数は1,000個以下で入力してください")]
    int? Quantity)
{
    /// <summary>
    /// 修正対象の商品識別ID(uuid)
    /// リクエストボディからは受け取らず、ルートパラメータの値をコントローラが設定する
    /// </summary>
    [JsonIgnore]
    public Guid ProductId { get; init; }
}