using Ec.Domain.Exceptions;
namespace Ec.Domain.Models;

/// <summary>
/// 注文を表すドメインエンティティ（注文集約のルート）
/// </summary>
/// <remarks>
/// EC側の注文は顧客が生成する主体である。
/// 管理サービス側では検索とステータス更新の対象にすぎなかったが、
/// EC側では「合計金額の算出」「初期ステータスの決定」という業務ルールを持つ。
/// </remarks>
public sealed class Order : Entity<Guid>
{
    /// <summary>
    /// 注文日時
    /// </summary>
    public DateTime OrderDate { get; }

    /// <summary>
    /// 合計金額
    /// </summary>
    /// <remarks>
    /// 注文明細の小計の合計と一致する。
    /// ordersテーブルに保存する値であり、注文時点の金額を記録する。
    /// </remarks>
    public int AmountTotal { get; }

    /// <summary>
    /// 注文した顧客
    /// </summary>
    public Customer Customer { get; }

    /// <summary>
    /// 注文ステータス
    /// </summary>
    public OrderStatus Status { get; }

    /// <summary>
    /// 支払い方法
    /// </summary>
    public PaymentMethod PaymentMethod { get; }

    /// <summary>
    /// 注文明細
    /// </summary>
    /// <remarks>
    /// 外部から要素を追加・削除できないようにIReadOnlyListで公開する。
    /// 注文明細は注文集約の一部であり、注文を経由せずに変更されてはならない。
    /// </remarks>
    public IReadOnlyList<OrderDetail> Details { get; }


    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <param name="orderDate">注文日時</param>
    /// <param name="amountTotal">合計金額</param>
    /// <param name="customer">注文した顧客</param>
    /// <param name="status">注文ステータス</param>
    /// <param name="paymentMethod">支払い方法</param>
    /// <param name="details">注文明細</param>
    /// <exception cref="DomainException">顧客・注文ステータス・支払い方法が未指定の場合、注文明細が空の場合、または合計金額が負数の場合</exception>
    public Order(
        Guid id,
        DateTime orderDate,
        int amountTotal,
        Customer customer,
        OrderStatus status,
        PaymentMethod paymentMethod,
        IReadOnlyList<OrderDetail> details) : base(id)
    {
        if (customer is null)
        {
            throw new DomainException("顧客が指定されていません。");
        }
        if (status is null)
        {
            throw new DomainException("注文ステータスが指定されていません。");
        }
        if (paymentMethod is null)
        {
            throw new DomainException("支払い方法が指定されていません。");
        }
        if (details is null || details.Count == 0)
        {
            throw new DomainException("注文明細が1件も指定されていません。");
        }
        if (amountTotal < 0)
        {
            throw new DomainException("合計金額に負の値は指定できません。");
        }
        OrderDate = orderDate;
        AmountTotal = amountTotal;
        Customer = customer;
        Status = status;
        PaymentMethod = paymentMethod;
        // 受け取ったコレクションをコピーする。
        // IReadOnlyListは「読み取り専用のビュー」であって「不変のコレクション」ではない。
        // 元のListへの参照が生きていると、呼び出し側が後からAddしたときに
        // 注文の明細が変わってしまう。集約の内部は外部から書き換えられてはならない
        Details = details.ToList();
    }

    /// <summary>
    /// 新しい注文を生成する（UC005）
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <param name="customer">注文する顧客</param>
    /// <param name="orderedStatus">初期の注文ステータス（注文済）</param>
    /// <param name="paymentMethod">支払い方法</param>
    /// <param name="details">注文明細</param>
    /// <param name="orderDate">注文日時</param>
    /// <returns>新しい注文</returns>
    /// <exception cref="DomainException">同じ商品が複数の明細に含まれる場合、その他コンストラクタの制約に反する場合</exception>
    /// <remarks>
    /// 合計金額は注文明細の小計から算出する。
    /// 呼び出し側に合計金額を渡させると、明細と合わない値を渡せてしまうため、
    /// 生成時に自ら計算することで整合性を保つ。
    /// </remarks>
    public static Order Create(
        Guid id,
        Customer customer,
        OrderStatus orderedStatus,
        PaymentMethod paymentMethod,
        IReadOnlyList<OrderDetail> details,
        DateTime orderDate)
    {
        if (details is null || details.Count == 0)
        {
            throw new DomainException("注文明細が1件も指定されていません。");
        }

        // 同じ商品が複数行に分かれていると、在庫の引き当てが行ごとに実行され、
        // 悲観的ロックを行っても意図しない結果になりうる。
        // カート側で1商品1行にまとめる前提だが、ドメインでも保証する。
        var duplicated = details
            .GroupBy(d => d.Product.Id)
            .Any(g => g.Count() > 1);
        if (duplicated)
        {
            throw new DomainException("同じ商品が複数の注文明細に含まれています。");
        }

        var amountTotal = details.Sum(d => d.Subtotal);
        return new Order(id, orderDate, amountTotal, customer, orderedStatus, paymentMethod, details);
    }
}