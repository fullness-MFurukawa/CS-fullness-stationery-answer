using Ec.Domain.Models;
namespace Ec.Application.Usecases;

/// <summary>
/// UC005:支払い方法一覧取得のユースケース
/// </summary>
public interface IPaymentMethodSearchUsecase
{
    /// <summary>
    /// すべての支払い方法を取得する
    /// </summary>
    /// <returns>支払い方法の一覧</returns>
    Task<IReadOnlyList<PaymentMethod>> ExecuteAsync();
}