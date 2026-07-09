using Backend.Application.Exceptions;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Usecases;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

namespace Backend.Application.Interactor;

/// <summary>
/// UC016:注文ステータス更新のユースケース実装
/// </summary>
public class OrderStatusUpdateInteractor : IOrderStatusUpdateUsecase
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderRepository">注文のリポジトリ</param>
    /// <param name="orderStatusRepository">注文ステータスのリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public OrderStatusUpdateInteractor(
        IOrderRepository orderRepository,
        IOrderStatusRepository orderStatusRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _orderStatusRepository = orderStatusRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定した注文のステータスを更新する
    /// </summary>
    /// <param name="param">注文ステータス更新の入力値</param>
    /// <returns>ステータスを更新した注文</returns>
    /// <exception cref="NotFoundException">対象の注文または注文ステータスが存在しない場合</exception>
    public async Task<Order> ExecuteAsync(OrderStatusUpdateParam param)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 更新対象の注文を取得する
            var current = await _orderRepository.FindByIdAsync(param.OrderId)
                ?? throw new NotFoundException("指定された注文は存在しません。");

            // 新しい注文ステータスを取得する
            var status = await _orderStatusRepository.FindByIdAsync(param.OrderStatusId)
                ?? throw new NotFoundException("指定された注文ステータスは存在しません。");

            await _orderRepository.UpdateStatusAsync(param.OrderId, status);

            // 識別子と他の項目は維持し、ステータスだけ差し替えた注文を返す
            return new Order(
                current.Id,
                current.OrderDate,
                current.AmountTotal,
                current.Customer,
                status,
                current.PaymentMethod,
                current.Details);
        });
    }
}