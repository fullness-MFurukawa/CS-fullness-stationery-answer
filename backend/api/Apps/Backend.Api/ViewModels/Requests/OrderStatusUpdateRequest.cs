using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Api.ViewModels.Requests;

/// <summary>
/// UC016:注文ステータス更新のリクエスト
/// </summary>
/// <param name="OrderStatusId">新しい注文ステータスID</param>
public sealed record OrderStatusUpdateRequest(
    [Required(ErrorMessage = "注文ステータスを選択してください")]
    [Range(1, int.MaxValue, ErrorMessage = "正しい注文ステータスを選択してください")]
    int? OrderStatusId)
{
    /// <summary>
    /// 更新対象の注文識別ID(uuid)
    /// リクエストボディからは受け取らず、ルートパラメータの値をコントローラが設定する
    /// </summary>
    [JsonIgnore]
    public Guid OrderId { get; init; }
}