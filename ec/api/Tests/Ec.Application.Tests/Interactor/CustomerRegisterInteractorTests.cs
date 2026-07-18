using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Application.Interfaces;
using Ec.Application.Params;
using Ec.Application.Tests.Fakes;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class CustomerRegisterInteractorTests
{
    private const string PlainPassword = "password123";
    private const string HashedPassword = "hashed-password";

    private Mock<ICustomerRepository> _customerRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private CustomerRegisterInteractor _interactor = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _customerRepository = new Mock<ICustomerRepository>();
        _customerRepository
            .Setup(r => r.ExistsByMailAddressAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _customerRepository
            .Setup(r => r.ExistsByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _customerRepository
            .Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);

        _passwordHasher = new Mock<IPasswordHasher>();
        _passwordHasher
            .Setup(h => h.HashPassword(PlainPassword))
            .Returns(HashedPassword);

        _interactor = new CustomerRegisterInteractor(
            _customerRepository.Object,
            _passwordHasher.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private static CustomerRegisterParam CreateParam(
        string mailAddress = "test@example.com",
        string username = "testuser")
        => new(
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            "テストビル101",
            "090-1234-5678",
            mailAddress,
            username,
            PlainPassword);

    [TestMethod(DisplayName = "顧客を登録し登録内容を返す")]
    public async Task ExecuteAsync_ValidParam_RegistersAndReturnsCustomer()
    {
        var customer = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual("テスト顧客", customer.Name);
        Assert.AreEqual("test@example.com", customer.MailAddress);
        Assert.AreEqual("testuser", customer.Username);
        Assert.AreEqual(HashedPassword, customer.Password);
        Assert.AreNotEqual(Guid.Empty, customer.Id);

        _customerRepository.Verify(
            r => r.AddAsync(It.Is<Customer>(c => c.MailAddress == "test@example.com")),
            Times.Once);
    }

    [TestMethod(DisplayName = "パスワードはハッシュ化してから集約に設定される")]
    public async Task ExecuteAsync_ValidParam_StoresHashedPassword()
    {
        Customer? saved = null;
        _customerRepository
            .Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .Callback<Customer>(c => saved = c)
            .Returns(Task.CompletedTask);

        await _interactor.ExecuteAsync(CreateParam());

        Assert.IsNotNull(saved);
        Assert.AreEqual(HashedPassword, saved!.Password);
        Assert.AreNotEqual(PlainPassword, saved.Password);
        _passwordHasher.Verify(h => h.HashPassword(PlainPassword), Times.Once);
    }

    [TestMethod(DisplayName = "メールアドレスが重複している場合はExistsExceptionをスローする")]
    public async Task ExecuteAsync_DuplicatedMailAddress_ThrowsExistsException()
    {
        _customerRepository
            .Setup(r => r.ExistsByMailAddressAsync("test@example.com"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "メールアドレスが重複している場合はアカウント名の確認も登録も行わない")]
    public async Task ExecuteAsync_DuplicatedMailAddress_DoesNotProceed()
    {
        _customerRepository
            .Setup(r => r.ExistsByMailAddressAsync("test@example.com"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _customerRepository.Verify(r => r.ExistsByUsernameAsync(It.IsAny<string>()), Times.Never);
        _customerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [TestMethod(DisplayName = "アカウント名が重複している場合はExistsExceptionをスローする")]
    public async Task ExecuteAsync_DuplicatedUsername_ThrowsExistsException()
    {
        _customerRepository
            .Setup(r => r.ExistsByUsernameAsync("testuser"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "アカウント名が重複している場合は登録を行わない")]
    public async Task ExecuteAsync_DuplicatedUsername_DoesNotRegister()
    {
        _customerRepository
            .Setup(r => r.ExistsByUsernameAsync("testuser"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _customerRepository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    [TestMethod(DisplayName = "識別IDはユースケースで採番される")]
    public async Task ExecuteAsync_CalledTwice_GeneratesDifferentIds()
    {
        var first = await _interactor.ExecuteAsync(CreateParam("a@example.com", "userone"));
        var second = await _interactor.ExecuteAsync(CreateParam("b@example.com", "usertwo"));

        Assert.AreNotEqual(first.Id, second.Id);
    }
}