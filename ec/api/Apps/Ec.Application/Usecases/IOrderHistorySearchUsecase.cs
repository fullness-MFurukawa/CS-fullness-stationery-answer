using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC007:購入履歴一覧取得のユースケース
/// </summary>
public interface IOrderHistorySearchUsecase
{
    /// <summary>
    /// 顧客の購入履歴を取得する
    /// </summary>
    /// <param name="customerId">顧客識別ID(uuid)。認証済みのトークンから取得する</param>
    /// <returns>該当する顧客の注文一覧（注文日時の降順）</returns>
    Task<IReadOnlyList<Order>> ExecuteAsync(Guid customerId);
}