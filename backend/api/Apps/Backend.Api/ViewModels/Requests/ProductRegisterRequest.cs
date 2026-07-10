using System.ComponentModel.DataAnnotations;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC010:商品登録のリクエスト
/// </summary>
/// <param name="Name">商品名</param>
/// <param name="Price">価格</param>
/// <param name="ImageUrl">画像URL</param>
/// <param name="CategoryId">商品カテゴリ識別ID(uuid)</param>
/// <param name="Quantity">在庫数</param>
public sealed record ProductRegisterRequest(
    [Required(ErrorMessage = "商品名を入力してください")]
    [StringLength(100, ErrorMessage = "商品名は100文字以内で入力してください")]
    string Name,

    [Required(ErrorMessage = "価格を入力してください")]
    [Range(0, int.MaxValue, ErrorMessage = "価格は0以上で入力してください")]
    int? Price,

    [StringLength(200, ErrorMessage = "画像URLは200文字以内で入力してください")]
    string? ImageUrl,

    [Required(ErrorMessage = "カテゴリを選択してください")]
    Guid? CategoryId,

    [Required(ErrorMessage = "在庫数を入力してください")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上で入力してください")]
    int? Quantity);