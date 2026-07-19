using Ec.Application.Exceptions;
using Ec.Application.Interfaces;
using Ec.Application.Params;
using Ec.Application.Usecases;
using Ec.Domain.Exceptions;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
namespace Ec.Application.Interactor;

/// <summary>
/// UC005:購入確定のユースケース実装
/// </summary>
/// <remarks>
/// EC側で最も複雑なユースケース。在庫の引き当てを伴うため、
/// 悲観的ロックとトランザクションで整合性を守る。
/// </remarks>
public class OrderCreateInteractor : IOrderCreateUsecase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IOrderStatusRepository _orderStatusRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="customerRepository">顧客のリポジトリ</param>
    /// <param name="productRepository">商品のリポジトリ</param>
    /// <param name="paymentMethodRepository">支払い方法のリポジトリ</param>
    /// <param name="orderStatusRepository">注文ステータスのリポジトリ</param>
    /// <param name="orderRepository">注文のリポジトリ</param>
    /// <param name="unitOfWork">トランザクション境界の制御</param>
    public OrderCreateInteractor(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IOrderStatusRepository orderStatusRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _orderStatusRepository = orderStatusRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 注文を確定する
    /// </summary>
    /// <param name="param">購入確定の入力値</param>
    /// <returns>確定した注文</returns>
    /// <exception cref="DomainException">注文明細が空、同じ商品が重複、または在庫が不足する場合</exception>
    /// <exception cref="NotFoundException">顧客・支払い方法・注文ステータス・商品のいずれかが存在しない場合</exception>
    public async Task<Order> ExecuteAsync(OrderCreateParam param)
    {
        if (param.Items is null || param.Items.Count == 0)
        {
            throw new DomainException("注文する商品が指定されていません。");
        }

        // 同じ商品が複数行に分かれていないことを、ロックを取る前に確認する。
        // 重複があると在庫の引き当てが行ごとに実行され、意図しない結果になる。
        var duplicated = param.Items
            .GroupBy(i => i.ProductId)
            .Any(g => g.Count() > 1);
        if (duplicated)
        {
            throw new DomainException("同じ商品が複数指定されています。");
        }

        // 在庫の引き当てを伴うため、トランザクション境界を制御する。
        // このブロック全体が1つのトランザクションであり、
        // 悲観的ロックはトランザクションの終了（コミットまたはロールバック）まで保持される。
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 認証済みのトークンから得た顧客IDで顧客を取得する
            var customer = await _customerRepository.FindByIdAsync(param.CustomerId)
                ?? throw new NotFoundException("顧客が存在しません。");

            // 支払い方法の実在を確認する
            var paymentMethod = await _paymentMethodRepository.FindByIdAsync(param.PaymentMethodId)
                ?? throw new NotFoundException("指定された支払い方法が存在しません。");

            // 初期ステータス（注文済）を取得する
            var orderedStatus = await _orderStatusRepository.FindByIdAsync(OrderStatus.OrderedId)
                ?? throw new NotFoundException("注文ステータスが存在しません。");

            // 対象商品を悲観的ロック付きで取得する。
            // ここで取得した在庫は、コミットまで他のトランザクションから更新されない。
            var productIds = param.Items.Select(i => i.ProductId).ToList();
            var lockedProducts = await _productRepository.FindByIdsForUpdateAsync(productIds);
            var productMap = lockedProducts.ToDictionary(p => p.Id);

            var details = new List<OrderDetail>();
            foreach (var item in param.Items)
            {
                // 取得できなかった商品は、存在しないか論理削除済み。
                // カートに入れた後に管理者が削除した場合もここで弾かれる。
                if (!productMap.TryGetValue(item.ProductId, out var product))
                {
                    throw new NotFoundException("指定された商品が存在しないか、販売を終了しました。");
                }

                // 在庫を引く。在庫不足ならProductStock.ReduceがDomainExceptionを投げ、
                // トランザクション全体がロールバックされる。
                var reduced = product.ReduceStock(item.Count);
                await _productRepository.UpdateStockAsync(reduced);

                // 注文明細は引く前の商品（価格が同じなので影響なし）で作る。
                // 単価はProduct.Priceから取得される
                details.Add(new OrderDetail(product, item.Count));
            }

            // 注文を組み立てる。合計金額は明細から自動算出される
            var order = Order.Create(
                Guid.NewGuid(),
                customer,
                orderedStatus,
                paymentMethod,
                details,
                DateTime.Now);

            await _orderRepository.AddAsync(order);
            return order;
        });
    }
}