using Ec.Api.ViewModels.Responses;
using Ec.Domain.Adapters;
using Ec.Domain.Models;
namespace Ec.Api.Adapters;

/// <summary>
/// 支払い方法のドメインオブジェクトとレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスは表示に必要な項目のみを持ち復元しないため、逆方向は未サポートとする。
/// </remarks>
public class PaymentMethodResponseAdapter : IEntityAdapter<PaymentMethodResponse, PaymentMethod>
{
    /// <summary>
    /// レスポンスからドメインオブジェクトへ変換する（未サポート）
    /// </summary>
    /// <param name="source">支払い方法のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public PaymentMethod ToDomain(PaymentMethodResponse source)
        => throw new NotSupportedException("レスポンスから支払い方法への復元は行えません。");

    /// <summary>
    /// ドメインオブジェクトからレスポンスへ変換する
    /// </summary>
    /// <param name="domain">ドメインの支払い方法</param>
    /// <returns>支払い方法のレスポンス</returns>
    public PaymentMethodResponse ToSource(PaymentMethod domain)
        => new(domain.Id, domain.Name);
}