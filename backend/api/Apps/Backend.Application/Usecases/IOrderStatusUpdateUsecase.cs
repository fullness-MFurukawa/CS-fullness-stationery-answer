using Backend.Application.Params;
using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// UC016:注文ステータス更新のユースケース
/// </summary>
public interface IOrderStatusUpdateUsecase
{
    /// <summary>
    /// 指定した注文のステータスを更新する
    /// </summary>
    /// <param name="param">注文ステータス更新の入力値</param>
    /// <returns>ステータスを更新した注文</returns>
    Task<Order> ExecuteAsync(OrderStatusUpdateParam param);
}