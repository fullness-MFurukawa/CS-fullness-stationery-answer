using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// 注文ステータス更新のリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// <see cref="IEntityAdapter{TSource, TDomain}"/> は本来ドメインオブジェクトとの双方向変換を表す。
/// ここでは <see cref="OrderStatusUpdateParam"/> をドメイン側の型として扱っているため、
/// 逆方向（入力値からリクエストへの復元）は用途が存在せず未サポートとする。
/// 全機能の実装後、片方向のアダプタへ切り出すことを検討する。
/// </remarks>
public class OrderStatusUpdateRequestAdapter : IEntityAdapter<OrderStatusUpdateRequest, OrderStatusUpdateParam>
{
    /// <summary>
    /// リクエストからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">注文ステータス更新のリクエスト（OrderIdはコントローラが設定済み）</param>
    /// <returns>注文ステータス更新の入力値</returns>
    public OrderStatusUpdateParam ToDomain(OrderStatusUpdateRequest source)
        => new(source.OrderId, source.OrderStatusId!.Value);

    /// <summary>
    /// ユースケースの入力値からリクエストへ変換する（未サポート）
    /// </summary>
    /// <param name="domain">注文ステータス更新の入力値</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public OrderStatusUpdateRequest ToSource(OrderStatusUpdateParam domain)
        => throw new NotSupportedException("入力値からリクエストへの変換は使用しません。");
}