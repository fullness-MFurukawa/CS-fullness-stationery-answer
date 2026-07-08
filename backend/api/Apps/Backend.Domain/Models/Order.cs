using Backend.Domain.Exceptions;
namespace Backend.Domain.Models;
/// <summary>
/// 注文を表すドメインエンティティ(注文集約のルート)
/// </summary>
public sealed class Order : Entity<Guid>
{
    private readonly List<OrderDetail> _details;

    /// <summary>
    /// 注文日
    /// </summary>
    public DateTime OrderDate { get; }

    /// <summary>
    /// 合計金額
    /// </summary>
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
    /// 注文明細の一覧
    /// </summary>
    public IReadOnlyList<OrderDetail> Details => _details;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="id">注文識別ID(uuid)</param>
    /// <param name="orderDate">注文日</param>
    /// <param name="amountTotal">合計金額</param>
    /// <param name="customer">注文した顧客</param>
    /// <param name="status">注文ステータス</param>
    /// <param name="paymentMethod">支払い方法</param>
    /// <param name="details">注文明細の一覧</param>
    /// <exception cref="DomainException">顧客・ステータス・支払い方法・明細が未指定、明細が空、または合計金額が負数の場合</exception>
    public Order(
        Guid id,
        DateTime orderDate,
        int amountTotal,
        Customer customer,
        OrderStatus status,
        PaymentMethod paymentMethod,
        IEnumerable<OrderDetail> details) : base(id)
    {
        if (customer is null)
        {
            throw new DomainException("注文の顧客が指定されていません。");
        }
        if (status is null)
        {
            throw new DomainException("注文ステータスが指定されていません。");
        }
        if (paymentMethod is null)
        {
            throw new DomainException("支払い方法が指定されていません。");
        }
        if (details is null)
        {
            throw new DomainException("注文明細が指定されていません。");
        }

        var list = details.ToList();
        if (list.Count == 0)
        {
            throw new DomainException("注文明細が1件も存在しません。");
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
        _details = list;
    }
}