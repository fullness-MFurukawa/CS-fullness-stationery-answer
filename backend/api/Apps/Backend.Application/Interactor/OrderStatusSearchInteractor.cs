using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// 注文ステータス一覧取得のユースケース実装
/// </summary>
public class OrderStatusSearchInteractor : IOrderStatusSearchUsecase
{
    private readonly IOrderStatusRepository _orderStatusRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderStatusRepository">注文ステータスのリポジトリ</param>
    public OrderStatusSearchInteractor(IOrderStatusRepository orderStatusRepository)
    {
        _orderStatusRepository = orderStatusRepository;
    }

    /// <summary>
    /// すべての注文ステータスを取得する
    /// </summary>
    /// <returns>注文ステータスの一覧</returns>
    public async Task<IReadOnlyList<OrderStatus>> ExecuteAsync()
    {
        return await _orderStatusRepository.FindAllAsync();
    }
}