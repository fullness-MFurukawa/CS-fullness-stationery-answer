using Backend.Application.Params;

namespace Backend.Application.Usecases;

/// <summary>
/// 補助:商品画像アップロードのユースケース
/// </summary>
/// <remarks>
/// 商品登録（UC010）・商品修正（UC012）の前段で使用する。
/// アップロードされた画像を保存し、商品の画像URLとして指定できるURLを返す。
/// </remarks>
public interface IImageUploadUsecase
{
    /// <summary>
    /// 画像をアップロードする
    /// </summary>
    /// <param name="param">商品画像アップロードの入力</param>
    /// <returns>保存された画像の公開URL</returns>
    /// <exception cref="Backend.Domain.Exceptions.DomainException">
    /// 画像が空、サイズが上限を超過、または対応していない形式の場合
    /// </exception>
    Task<string> ExecuteAsync(ImageUploadParam param);
}