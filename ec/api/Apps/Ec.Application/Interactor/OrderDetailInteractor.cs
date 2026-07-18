using Ec.Application.Exceptions;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC007:購入履歴詳細取得のユースケース実装
/// </summary>
public class OrderDetailInteractor : IOrderDetailUsecase
{
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="orderRepository">注文のリポジトリ</param>
    public OrderDetailInteractor(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <summary>
    /// 注文の詳細を取得する
    /// </summary>
    /// <param name="param">購入履歴詳細取得の入力値</param>
    /// <returns>該当する注文</returns>
    /// <exception cref="NotFoundException">注文が存在しない、または要求元の顧客の注文でない場合</exception>
    /// <remarks>
    /// リポジトリが注文識別IDと顧客識別IDの両方で絞り込むため、
    /// 他人の注文は取得できずnullが返る。
    /// 「存在しない」と「他人の注文」を区別せず、どちらも同じNotFoundとすることで、
    /// 注文識別IDの推測による他人の注文の存在有無の推測を防ぐ。
    /// </remarks>
    public async Task<Order> ExecuteAsync(OrderDetailParam param)
        => await _orderRepository.FindByIdAsync(param.OrderId, param.CustomerId)
            ?? throw new NotFoundException("指定された注文が存在しません。");
}