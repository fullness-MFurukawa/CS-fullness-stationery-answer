using Ec.Domain.Models;
namespace Ec.Domain.Repositories;

/// <summary>
/// 注文ステータスの永続化を担うリポジトリ
/// </summary>
/// <remarks>
/// マスタデータであり、EC側は参照のみを行う。
/// 注文ステータスの更新は管理サービス側（UC017）の責務である。
/// </remarks>
public interface IOrderStatusRepository
{
    /// <summary>
    /// 識別IDを指定して注文ステータスを取得
    /// </summary>
    /// <param name="id">注文ステータス識別ID</param>
    /// <returns>該当する注文ステータス。存在しない場合はnull</returns>
    /// <remarks>
    /// 購入確定（UC005）で、初期ステータス（注文済）を取得するために用いる。
    /// <see cref="OrderStatus.OrderedId"/>を指定して呼び出す。
    /// </remarks>
    Task<OrderStatus?> FindByIdAsync(int id);
}