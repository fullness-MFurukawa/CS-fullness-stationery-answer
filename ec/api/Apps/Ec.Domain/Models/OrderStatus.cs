using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 注文ステータスを表すドメインエンティティ
/// </summary>
/// <remarks>
/// 識別子はUUIDではなく連番(int)である。理由は<see cref="PaymentMethod"/>と同じ。
/// EC側では注文ステータスの更新は行わない（管理サービス側UC017の責務）。
/// 購入確定（UC005）で初期値を設定し、購入履歴（UC007）で表示するのみである。
/// </remarks>
public sealed class OrderStatus : Entity<int>
{
    /// <summary>
    /// 注文時に設定される初期ステータス（注文済）の識別ID
    /// </summary>
    /// <remarks>
    /// 購入確定（UC005）で注文を作成する際に用いる。
    /// order_statusテーブルの先頭のレコードに対応する。
    /// </remarks>
    public const int OrderedId = 1;

    /// <summary>
    /// 注文ステータス名
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">注文ステータス識別ID</param>
    /// <param name="name">注文ステータス名</param>
    /// <exception cref="DomainException">注文ステータス名が未指定の場合</exception>
    public OrderStatus(int id, string name) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("注文ステータス名が指定されていません。");
        }
        Name = name;
    }
}