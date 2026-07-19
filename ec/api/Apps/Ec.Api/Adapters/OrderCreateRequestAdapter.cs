using Ec.Api.ViewModels.Requests;
using Ec.Application.Params;
namespace Ec.Api.Adapters;

/// <summary>
/// 購入確定のリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// 顧客識別IDはリクエストに含まれず、認証済みのトークンから得た値を
/// コントローラが渡す。そのためIEntityAdapterの単一引数の形には収まらず、
/// 専用のメソッドとして実装する。
/// </remarks>
public class OrderCreateRequestAdapter
{
    /// <summary>
    /// リクエストと顧客識別IDからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">購入確定のリクエスト</param>
    /// <param name="customerId">認証済みの顧客識別ID(uuid)</param>
    /// <returns>購入確定の入力値</returns>
    public OrderCreateParam ToParam(OrderCreateRequest source, Guid customerId)
        => new(
            customerId,
            source.PaymentMethodId!.Value,
            source.Items
                .Select(i => new OrderItemParam(i.ProductId!.Value, i.Count))
                .ToList());
}