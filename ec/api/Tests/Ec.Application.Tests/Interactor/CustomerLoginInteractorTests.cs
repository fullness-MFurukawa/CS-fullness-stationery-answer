using System.Security.Claims;
using Ec.Application.Exceptions;
using Ec.Application.Interactor;
using Ec.Application.Interfaces;
using Ec.Application.Params;
using Ec.Application.Results;
using Ec.Domain.Models;
using Ec.Domain.Repositories;
using Moq;
namespace Ec.Application.Tests.Interactor;

[TestClass]
[TestCategory("Ec.Application.Interactor")]
public class CustomerLoginInteractorTests
{
    private const string MailAddress = "test@example.com";
    private const string PlainPassword = "password123";
    private const string HashedPassword = "hashed-password";

    private Mock<ICustomerRepository> _customerRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private CustomerLoginInteractor _interactor = null!;
    private Customer _customer = null!;
    private AccessToken _token = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _customer = new Customer(
            Guid.NewGuid(),
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            null,
            "090-1234-5678",
            MailAddress,
            "testuser",
            HashedPassword,
            DateTime.UtcNow);

        _token = new AccessToken("dummy-token", DateTimeOffset.UtcNow.AddMinutes(30));

        _customerRepository = new Mock<ICustomerRepository>();
        _customerRepository
            .Setup(r => r.FindByMailAddressAsync(MailAddress))
            .ReturnsAsync(_customer);

        _passwordHasher = new Mock<IPasswordHasher>();
        _passwordHasher
            .Setup(h => h.VerifyPassword(HashedPassword, PlainPassword))
            .Returns(true);

        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();
        _accessTokenGenerator
            .Setup(g => g.Generate(It.IsAny<IEnumerable<Claim>>()))
            .Returns(_token);

        _interactor = new CustomerLoginInteractor(
            _customerRepository.Object,
            _passwordHasher.Object,
            _accessTokenGenerator.Object);
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private static CustomerLoginParam CreateParam(
        string mailAddress = MailAddress,
        string password = PlainPassword)
        => new(mailAddress, password);

    [TestMethod(DisplayName = "認証に成功すると顧客とトークンを返す")]
    public async Task ExecuteAsync_ValidCredentials_ReturnsResult()
    {
        var result = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual(_customer, result.Customer);
        Assert.AreEqual(_token, result.Token);
    }

    [TestMethod(DisplayName = "トークンには顧客のロールが含まれる")]
    public async Task ExecuteAsync_ValidCredentials_IncludesCustomerRole()
    {
        IEnumerable<Claim>? capturedClaims = null;
        _accessTokenGenerator
            .Setup(g => g.Generate(It.IsAny<IEnumerable<Claim>>()))
            .Callback<IEnumerable<Claim>>(c => capturedClaims = c.ToList())
            .Returns(_token);

        await _interactor.ExecuteAsync(CreateParam());

        Assert.IsNotNull(capturedClaims);
        var claims = capturedClaims!.ToList();

        // 顧客のトークンで管理サービスのAPIを呼べないよう、ロールを含める
        var role = claims.FirstOrDefault(c => c.Type == "role");
        Assert.IsNotNull(role);
        Assert.AreEqual("customer", role!.Value);

        // 顧客識別IDはsubクレームに含める
        var sub = claims.FirstOrDefault(c => c.Type == "sub");
        Assert.IsNotNull(sub);
        Assert.AreEqual(_customer.Id.ToString(), sub!.Value);
    }

    [TestMethod(DisplayName = "メールアドレスが存在しない場合はAuthenticationFailedExceptionをスローする")]
    public async Task ExecuteAsync_MailAddressNotFound_ThrowsAuthenticationFailedException()
    {
        _customerRepository
            .Setup(r => r.FindByMailAddressAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "メールアドレスが存在しない場合もダミー照合を行いトークンは生成しない")]
    public async Task ExecuteAsync_MailAddressNotFound_VerifiesDummyAndDoesNotGenerateToken()
    {
        _customerRepository
            .Setup(r => r.FindByMailAddressAsync(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        // 応答時間からアカウントの存在を推測されないよう、ダミーで照合している
        _passwordHasher.Verify(
            h => h.VerifyPassword(It.IsAny<string>(), PlainPassword),
            Times.Once);
        _accessTokenGenerator.Verify(g => g.Generate(It.IsAny<IEnumerable<Claim>>()), Times.Never);
    }

    [TestMethod(DisplayName = "パスワードが一致しない場合はAuthenticationFailedExceptionをスローする")]
    public async Task ExecuteAsync_WrongPassword_ThrowsAuthenticationFailedException()
    {
        _passwordHasher
            .Setup(h => h.VerifyPassword(HashedPassword, It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(CreateParam(password: "wrong")));
    }

    [TestMethod(DisplayName = "パスワードが一致しない場合はトークンを生成しない")]
    public async Task ExecuteAsync_WrongPassword_DoesNotGenerateToken()
    {
        _passwordHasher
            .Setup(h => h.VerifyPassword(HashedPassword, It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(CreateParam(password: "wrong")));

        _accessTokenGenerator.Verify(g => g.Generate(It.IsAny<IEnumerable<Claim>>()), Times.Never);
    }
}