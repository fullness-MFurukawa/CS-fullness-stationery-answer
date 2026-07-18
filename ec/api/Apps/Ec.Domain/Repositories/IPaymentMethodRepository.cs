using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 支払い方法の永続化を担うリポジトリ
/// </summary>
/// <remarks>
/// マスタデータであり、EC側・管理サービス側ともに参照のみを行う。
/// </remarks>
public interface IPaymentMethodRepository
{
    /// <summary>
    /// すべての支払い方法を取得
    /// </summary>
    /// <returns>支払い方法の一覧</returns>
    /// <remarks>
    /// 購入確認画面（FP009）のプルダウンに表示する。
    /// UC005の時点では「現金」のみが登録されている想定である。
    /// </remarks>
    Task<IReadOnlyList<PaymentMethod>> FindAllAsync();

    /// <summary>
    /// 識別IDを指定して支払い方法を取得
    /// </summary>
    /// <param name="id">支払い方法識別ID</param>
    /// <returns>該当する支払い方法。存在しない場合はnull</returns>
    /// <remarks>
    /// 購入確定（UC005）で、指定された支払い方法が実在するかの確認に用いる。
    /// 存在しない識別IDが指定された場合は404を返す。
    /// </remarks>
    Task<PaymentMethod?> FindByIdAsync(int id);
}