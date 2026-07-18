using Ec.Application.Params;
using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC005:購入確定のユースケース
/// </summary>
public interface IOrderCreateUsecase
{
    /// <summary>
    /// 注文を確定する
    /// </summary>
    /// <param name="param">購入確定の入力値</param>
    /// <returns>確定した注文</returns>
    Task<Order> ExecuteAsync(OrderCreateParam param);
}