using System.ComponentModel.DataAnnotations;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC014:商品カテゴリ登録のリクエスト
/// </summary>
/// <param name="Name">商品カテゴリ名</param>
public sealed record CategoryRegisterRequest(
    [Required(ErrorMessage = "商品カテゴリ名を入力してください")]
    [StringLength(30, ErrorMessage = "商品カテゴリ名は30文字以内で入力してください")]
    string Name);