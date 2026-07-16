using Backend.Api.ViewModels.Requests;
using Backend.Application.Params;
using Backend.Domain.Adapters;

namespace Backend.Api.Adapters;

/// <summary>
/// 商品修正のリクエストをユースケースの入力値へ変換するアダプタ
/// </summary>
/// <remarks>
/// <see cref="IEntityAdapter{TSource, TDomain}"/> は本来ドメインオブジェクトとの双方向変換を表す。
/// ここでは <see cref="ProductUpdateParam"/> をドメイン側の型として扱っているため、
/// 逆方向(入力値からリクエストへの復元)は用途が存在せず未サポートとする。
/// 全機能の実装後、片方向のアダプタへ切り出すことを検討する。
/// </remarks>
public class ProductUpdateRequestAdapter : IEntityAdapter<ProductUpdateRequest, ProductUpdateParam>
{
    /// <summary>
    /// リクエストからユースケースの入力値へ変換する
    /// </summary>
    /// <param name="source">商品修正のリクエスト（ProductIdはコントローラが設定済み）</param>
    /// <returns>商品修正の入力値</returns>
    /// <remarks>
    /// 画像ファイルが指定された場合は、ストリームとメタ情報を入力値へ引き渡す。
    /// 未指定かつ削除指定もない場合は既存の画像を維持する。
    /// </remarks>
    public ProductUpdateParam ToDomain(ProductUpdateRequest source)
    {
        var image = source.Image;

        return new ProductUpdateParam(
            source.ProductId,
            source.Name,
            source.Price!.Value,
            source.CategoryId!.Value,
            source.Quantity!.Value,
            image?.OpenReadStream(),
            image?.FileName,
            image?.ContentType,
            image?.Length ?? 0,
            source.RemoveImage);
    }

    /// <summary>
    /// ユースケースの入力値からリクエストへ変換する（未サポート）
    /// </summary>
    /// <param name="domain">商品修正の入力値</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">この方向の変換は使用しない</exception>
    public ProductUpdateRequest ToSource(ProductUpdateParam domain)
        => throw new NotSupportedException("入力値からリクエストへの変換は使用しません。");
}