using Backend.Domain.Models;
namespace Backend.Domain.Repositories;

/// <summary>
/// 注文ステータスの参照を担うリポジトリ
/// </summary>
public interface IOrderStatusRepository
{
    /// <summary>
    /// すべての注文ステータスを取得
    /// </summary>
    /// <returns>注文ステータスの一覧</returns>
    Task<IReadOnlyList<OrderStatus>> FindAllAsync();

    /// <summary>
    /// IDを指定して注文ステータスを取得
    /// </summary>
    /// <param name="id">注文ステータスID</param>
    /// <returns>該当する注文ステータス。存在しない場合はnull</returns>
    Task<OrderStatus?> FindByIdAsync(int id);
}