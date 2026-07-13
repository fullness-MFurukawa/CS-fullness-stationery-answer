using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC012:商品修正のリクエスト
/// </summary>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">在庫数</param>
/// <param name="Image">差し替える商品画像ファイル。未指定の場合は既存の画像を維持する</param>
/// <remarks>
/// 画像ファイルを含むため multipart/form-data で受け取る。
/// </remarks>
public sealed record ProductUpdateRequest(
    [Required(ErrorMessage = "商品名を入力してください")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください")]
    string Name,

    [Required(ErrorMessage = "価格を入力してください")]
    [Range(0, int.MaxValue, ErrorMessage = "価格は0以上で入力してください")]
    int? Price,

    [Required(ErrorMessage = "カテゴリを選択してください")]
    Guid? CategoryId,

    [Required(ErrorMessage = "在庫数を入力してください")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上で入力してください")]
    int? Quantity,

    IFormFile? Image)
{
    /// <summary>
    /// 修正対象の商品識別ID(uuid)
    /// リクエストボディからは受け取らず、ルートパラメータの値をコントローラが設定する
    /// </summary>
    [JsonIgnore]
    public Guid ProductId { get; init; }
}