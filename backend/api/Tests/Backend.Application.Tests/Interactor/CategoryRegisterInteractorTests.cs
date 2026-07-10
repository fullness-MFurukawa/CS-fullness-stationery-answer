using Backend.Application.Interactor;
using Backend.Application.Tests.Fakes;
using Backend.Domain.Exceptions;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class CategoryRegisterInteractorTests
{
    private Mock<IProductCategoryRepository> _productCategoryRepository = null!;
    private CategoryRegisterInteractor _interactor = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _productCategoryRepository = new Mock<IProductCategoryRepository>();
        _productCategoryRepository
            .Setup(r => r.AddAsync(It.IsAny<ProductCategory>()))
            .Returns(Task.CompletedTask);

        _interactor = new CategoryRegisterInteractor(
            _productCategoryRepository.Object,
            new PassThroughUnitOfWork());
    }

    [TestMethod(DisplayName = "商品カテゴリを登録し登録内容を返す")]
    public async Task ExecuteAsync_ValidName_RegistersAndReturnsCategory()
    {
        var category = await _interactor.ExecuteAsync("文房具");

        Assert.AreEqual("文房具", category.Name);
        Assert.AreNotEqual(Guid.Empty, category.Id);

        _productCategoryRepository.Verify(
            r => r.AddAsync(It.Is<ProductCategory>(c => c.Name == "文房具")),
            Times.Once);
    }

    [TestMethod(DisplayName = "識別IDはユースケースで採番される")]
    public async Task ExecuteAsync_CalledTwice_GeneratesDifferentIds()
    {
        var first = await _interactor.ExecuteAsync("文房具");
        var second = await _interactor.ExecuteAsync("雑貨");

        Assert.AreNotEqual(first.Id, second.Id);
    }

    [TestMethod(DisplayName = "カテゴリ名が未指定ならDomainExceptionをスローする")]
    public async Task ExecuteAsync_EmptyName_ThrowsDomainException()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(() => _interactor.ExecuteAsync(""));
    }

    [TestMethod(DisplayName = "カテゴリ名が未指定ならリポジトリを呼び出さない")]
    public async Task ExecuteAsync_EmptyName_DoesNotCallRepository()
    {
        await Assert.ThrowsExactlyAsync<DomainException>(() => _interactor.ExecuteAsync(""));

        _productCategoryRepository.Verify(
            r => r.AddAsync(It.IsAny<ProductCategory>()),
            Times.Never);
    }
}