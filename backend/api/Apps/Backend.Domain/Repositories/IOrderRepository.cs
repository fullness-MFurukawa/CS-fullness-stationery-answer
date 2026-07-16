using Backend.Domain.Models;

namespace Backend.Domain.Repositories;

/// <summary>
/// 注文(注文集約)の永続化を担うリポジトリ
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// すべての注文を取得（新しい順）
    /// </summary>
    /// <returns>注文の一覧</returns>
    Task<IReadOnlyList<Order>> FindAllAsync();

    /// <summary>
    /// 購入日・顧客アカウント名・注文ステータスで注文を検索
    /// </summary>
    /// <param name="orderDate">購入日。指定しない場合はnull</param>
    /// <param name="customerAccountName">顧客アカウント名。指定しない場合はnull</param>
    /// <param name="orderStatusId">注文ステータスID。指定しない場合はnull</param>
    /// <returns>条件に一致する注文の一覧</returns>
    Task<IReadOnlyList<Order>> SearchAsync(DateOnly? orderDate, string? customerAccountName, int? orderStatusId);

    /// <summary>
    /// 識別IDを指定して注文を取得
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <returns>該当する注文。存在しない場合はnull</returns>
    Task<Order?> FindByIdAsync(Guid id);

    /// <summary>
    /// 注文ステータスを更新
    /// </summary>
    /// <param name="orderId">注文識別ID(uuid)</param>
    /// <param name="status">新しい注文ステータス</param>
    Task UpdateStatusAsync(Guid orderId, OrderStatus status);

    /// <summary>
    /// 注文の件数を取得
    /// </summary>
    /// <returns>注文の件数</returns>
    Task<int> CountAsync();

    /// <summary>
    /// すべての注文の合計金額を集計
    /// </summary>
    /// <returns>合計金額の総和</returns>
    Task<int> SumAmountTotalAsync();

    /// <summary>
    /// 注文ステータスごとの注文件数を集計
    /// </summary>
    /// <returns>注文ステータスIDをキー、件数を値とする辞書</returns>
    Task<IReadOnlyDictionary<int, int>> CountByStatusAsync();
}