using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC015:購入履歴検索のユースケース
/// </summary>
public interface IOrderHistorySearchUsecase
{
    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    /// <param name="param">購入履歴検索の入力値</param>
    /// <returns>条件に一致する注文の一覧（新しい順）</returns>
    Task<IReadOnlyList<Order>> ExecuteAsync(OrderHistorySearchParam param);
}