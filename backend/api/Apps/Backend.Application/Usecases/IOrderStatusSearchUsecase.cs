using Backend.Domain.Models;

namespace Backend.Application.Usecases;

/// <summary>
/// 注文ステータス一覧取得のユースケース
/// 注文ステータス更新画面でステータスの選択肢として使用する
/// </summary>
public interface IOrderStatusSearchUsecase
{
    /// <summary>
    /// すべての注文ステータスを取得する
    /// </summary>
    /// <returns>注文ステータスの一覧</returns>
    Task<IReadOnlyList<OrderStatus>> ExecuteAsync();
}