using Ec.Application.Usecases;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC005:支払い方法一覧取得のユースケース実装
/// </summary>
public class PaymentMethodSearchInteractor : IPaymentMethodSearchUsecase
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="paymentMethodRepository">支払い方法のリポジトリ</param>
    public PaymentMethodSearchInteractor(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    /// <summary>
    /// すべての支払い方法を取得する
    /// </summary>
    /// <returns>支払い方法の一覧</returns>
    public async Task<IReadOnlyList<PaymentMethod>> ExecuteAsync()
        => await _paymentMethodRepository.FindAllAsync();
}