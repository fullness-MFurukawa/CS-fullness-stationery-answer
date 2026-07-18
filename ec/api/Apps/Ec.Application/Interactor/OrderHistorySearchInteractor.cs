using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC007:購入履歴一覧取得のユースケース実装
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
    /// 顧客の購入履歴を取得する
    /// </summary>
    /// <param name="customerId">顧客識別ID(uuid)。認証済みのトークンから取得する</param>
    /// <returns>該当する顧客の注文一覧（注文日時の降順）</returns>
    public async Task<IReadOnlyList<Order>> ExecuteAsync(Guid customerId)
        => await _orderRepository.FindByCustomerAsync(customerId);
}