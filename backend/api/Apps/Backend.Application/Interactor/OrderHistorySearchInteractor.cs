using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC015:購入履歴検索のユースケース実装
/// </summary>
public class OrderHistorySearchInteractor : IOrderHistorySearchUsecase
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderRepository">注文のリポジトリ</param>
    public OrderHistorySearchInteractor(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 購入日または顧客アカウント名で購入履歴を検索する
    /// </summary>
    /// <param name="param">購入履歴検索の入力値</param>
    /// <returns>条件に一致する注文の一覧（新しい順）</returns>
    public async Task<IReadOnlyList<Order>> ExecuteAsync(OrderHistorySearchParam param)
    {
        // 条件が未指定（両方null）の場合は全件が返る
        return await _orderRepository.SearchAsync(param.OrderDate, param.CustomerAccountName);
    }
}