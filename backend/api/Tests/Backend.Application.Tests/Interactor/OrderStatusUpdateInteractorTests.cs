using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Params;
using Backend.Application.Tests.Fakes;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class OrderStatusUpdateInteractorTests
{
    private Mock<IOrderRepository> _orderRepository = null!;
    private Mock<IOrderStatusRepository> _orderStatusRepository = null!;
    private OrderStatusUpdateInteractor _interactor = null!;

    private Order _existing = null!;
    private OrderStatus _newStatus = null!;

    /// <summary>
    /// テスト用の注文を生成する（ステータスは「注文済」）
    /// </summary>
    private static Order CreateOrder()
    {
        var category = new ProductCategory(Guid.NewGuid(), "文房具");
        var product = new Product(
            Guid.NewGuid(), "水性ボールペン(黒)", 120, null, category,
            new ProductStock(Guid.NewGuid(), 10));
        var detail = new OrderDetail(1, product, 2);
        var customer = new Customer(
            Guid.NewGuid(), "テスト顧客", "テストコキャク", "東京都新宿区", "テストビル101",
            "090-1234-5678", "test@example.com", "testuser", "hashed-password", DateTime.Now);

        return new Order(
            Guid.NewGuid(),
            new DateTime(2024, 5, 12, 15, 30, 0),
            240,
            customer,
            new OrderStatus(1, "注文済"),
            new PaymentMethod(1, "現金"),
            [detail]);
    }

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _existing = CreateOrder();
        _newStatus = new OrderStatus(2, "入金済");

        _orderRepository = new Mock<IOrderRepository>();
        _orderRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_existing);
        _orderRepository
            .Setup(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);

        _orderStatusRepository = new Mock<IOrderStatusRepository>();
        _orderStatusRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(_newStatus);

        _interactor = new OrderStatusUpdateInteractor(
            _orderRepository.Object,
            _orderStatusRepository.Object,
            new PassThroughUnitOfWork());
    }

    [TestMethod(DisplayName = "注文ステータスを更新しリポジトリの更新処理を呼び出す")]
    public async Task ExecuteAsync_ValidParam_CallsUpdateStatus()
    {
        await _interactor.ExecuteAsync(new OrderStatusUpdateParam(_existing.Id, 2));

        _orderRepository.Verify(r => r.UpdateStatusAsync(_existing.Id, _newStatus), Times.Once);
    }

    [TestMethod(DisplayName = "更新後のステータスを反映した注文を返す")]
    public async Task ExecuteAsync_ValidParam_ReturnsOrderWithNewStatus()
    {
        var order = await _interactor.ExecuteAsync(new OrderStatusUpdateParam(_existing.Id, 2));

        Assert.AreEqual(2, order.Status.Id);
        Assert.AreEqual("入金済", order.Status.Name);
    }

    [TestMethod(DisplayName = "ステータス以外の項目は更新後も維持される")]
    public async Task ExecuteAsync_ValidParam_PreservesOtherValues()
    {
        var order = await _interactor.ExecuteAsync(new OrderStatusUpdateParam(_existing.Id, 2));

        Assert.AreEqual(_existing.Id, order.Id);
        Assert.AreEqual(_existing.OrderDate, order.OrderDate);
        Assert.AreEqual(_existing.AmountTotal, order.AmountTotal);
        Assert.AreEqual(_existing.Customer, order.Customer);
        Assert.AreEqual(_existing.PaymentMethod, order.PaymentMethod);
        Assert.HasCount(1, order.Details);
    }

    [TestMethod(DisplayName = "注文が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_OrderNotFound_ThrowsNotFoundException()
    {
        _orderRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new OrderStatusUpdateParam(Guid.NewGuid(), 2)));

        _orderRepository.Verify(
            r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()),
            Times.Never);
    }

    [TestMethod(DisplayName = "注文が存在しない場合はステータスの取得も行わない")]
    public async Task ExecuteAsync_OrderNotFound_DoesNotFetchStatus()
    {
        _orderRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new OrderStatusUpdateParam(Guid.NewGuid(), 2)));

        _orderStatusRepository.Verify(r => r.FindByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [TestMethod(DisplayName = "注文ステータスが存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_StatusNotFound_ThrowsNotFoundException()
    {
        _orderStatusRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((OrderStatus?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(new OrderStatusUpdateParam(_existing.Id, 999)));

        _orderRepository.Verify(
            r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()),
            Times.Never);
    }
}