using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// 新商品登録のリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// <see cref="IEntityAdapter{TSource, TDomain}"/> は本来ドメインオブジェクトとの双方向変換を表す。
/// ここでは <see cref="ProductRegisterParam"/> をドメイン側の型として扱っているため、
/// 逆方向（入力値からリクエストへの復元）は用途が存在せず未サポートとする。
/// 全機能の実装後、片方向のアダプタへ切り出すことを検討する。
/// </remarks>
public class ProductRegisterRequestAdapter : IEntityAdapter<ProductRegisterRequest, ProductRegisterParam>
{
    /// <summary>
    /// リクエストからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">新商品登録のリクエスト</param>
    /// <returns>新商品登録の入力値</returns>
    /// <remarks>
    /// 必須項目はモデル検証を通過している前提のため、nullable の値を確定して取り出す。
    /// </remarks>
    public ProductRegisterParam ToDomain(ProductRegisterRequest source)
        => new(
            source.Name,
            source.Price!.Value,
            source.ImageUrl,
            source.CategoryId!.Value,
            source.Quantity!.Value);

    /// <summary>
    /// ユースケースの入力値からリクエストへ変換する（未サポート）
    /// </summary>
    /// <param name="domain">新商品登録の入力値</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public ProductRegisterRequest ToSource(ProductRegisterParam domain)
        => throw new NotSupportedException("入力値からリクエストへの変換は使用しません。");
}