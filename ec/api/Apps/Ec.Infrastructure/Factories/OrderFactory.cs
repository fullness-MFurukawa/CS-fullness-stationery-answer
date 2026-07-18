using Ec.Domain.Adapters;
using Ec.Domain.Exceptions;
using Ec.Infrastructure.Adapters;

using DomainOrder = Ec.Domain.Models.Order;
using DomainOrderDetail = Ec.Domain.Models.OrderDetail;
using EfOrder = Ec.Infrastructure.Entities.Order;

namespace Ec.Infrastructure.Factories;

/// <summary>
/// EFの注文エンティティからドメインの注文集約を組み立てるファクトリ
/// </summary>
public class OrderFactory : IAggregateFactory<EfOrder, DomainOrder>
{
    private readonly CustomerAdapter _customerAdapter;
    private readonly OrderStatusAdapter _orderStatusAdapter;
    private readonly PaymentMethodAdapter _paymentMethodAdapter;
    private readonly ProductFactory _productFactory;
    private readonly OrderDetailAdapter _orderDetailAdapter;
    private readonly OrderAdapter _orderAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerAdapter">顧客のアダプタ</param>
    /// <param name="orderStatusAdapter">注文ステータスのアダプタ</param>
    /// <param name="paymentMethodAdapter">支払い方法のアダプタ</param>
    /// <param name="productFactory">商品集約を組み立てるファクトリ</param>
    /// <param name="orderDetailAdapter">注文明細のアダプタ</param>
    /// <param name="orderAdapter">注文のアダプタ</param>
    public OrderFactory(
        CustomerAdapter customerAdapter,
        OrderStatusAdapter orderStatusAdapter,
        PaymentMethodAdapter paymentMethodAdapter,
        ProductFactory productFactory,
        OrderDetailAdapter orderDetailAdapter,
        OrderAdapter orderAdapter)
    {
        _customerAdapter = customerAdapter;
        _orderStatusAdapter = orderStatusAdapter;
        _paymentMethodAdapter = paymentMethodAdapter;
        _productFactory = productFactory;
        _orderDetailAdapter = orderDetailAdapter;
        _orderAdapter = orderAdapter;
    }

    /// <summary>
    /// EFの注文エンティティ（顧客・ステータス・支払い方法・明細・商品を Include 済み）から注文集約を組み立てる
    /// </summary>
    /// <param name="source">組み立て元のEFエンティティ</param>
    /// <returns>注文集約（ドメイン）</returns>
    /// <exception cref="DomainException">関連が未ロードの場合</exception>
    public DomainOrder Create(EfOrder source)
    {
        // 関連が未ロードなら組み立て不可
        if (source.Customer is null)
        {
            throw new DomainException("顧客が読み込まれていません。");
        }
        if (source.OrderStatus is null)
        {
            throw new DomainException("注文ステータスが読み込まれていません。");
        }
        if (source.PaymentMethod is null)
        {
            throw new DomainException("支払い方法が読み込まれていません。");
        }
        if (source.OrderDetails.Count == 0)
        {
            throw new DomainException("注文明細が読み込まれていません。");
        }

        // 関連を各Adapterで変換
        var customer = _customerAdapter.ToDomain(source.Customer);
        var status = _orderStatusAdapter.ToDomain(source.OrderStatus);
        var paymentMethod = _paymentMethodAdapter.ToDomain(source.PaymentMethod);

        // 明細は商品集約を組み立ててから変換
        var details = new List<DomainOrderDetail>();
        foreach (var detail in source.OrderDetails)
        {
            if (detail.Product is null)
            {
                throw new DomainException("注文明細の商品が読み込まれていません。");
            }

            var product = _productFactory.Create(detail.Product);
            details.Add(_orderDetailAdapter.ToDomain(detail, product));
        }

        // 変換結果を使って注文集約を組み立てる
        return _orderAdapter.ToDomain(source, customer, status, paymentMethod, details);
    }
}