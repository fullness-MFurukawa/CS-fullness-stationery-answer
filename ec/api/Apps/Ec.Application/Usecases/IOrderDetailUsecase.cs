using Ec.Application.Params;
using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC007:購入履歴詳細取得のユースケース
/// </summary>
public interface IOrderDetailUsecase
{
    /// <summary>
    /// 注文の詳細を取得する
    /// </summary>
    /// <param name="param">購入履歴詳細取得の入力値</param>
    /// <returns>該当する注文</returns>
    Task<Order> ExecuteAsync(OrderDetailParam param);
}