using System.Security.Claims;

using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Results;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class EmployeeLoginInteractorTests
{
    /// <summary>
    /// トークン生成のモックが返す固定のアクセストークン
    /// </summary>
    private static readonly AccessToken DummyToken =
        new("dummy-token", DateTimeOffset.UtcNow.AddMinutes(30));

    private Mock<IEmployeeAccountRepository> _employeeAccountRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private Mock<IAccessTokenGenerator> _accessTokenGenerator = null!;
    private EmployeeLoginInteractor _interactor = null!;

    /// <summary>
    /// テスト用の社員アカウントを生成する
    /// </summary>
    private static EmployeeAccount CreateAccount()
        => new(
            Guid.NewGuid(),
            "fullness",
            "hashed-password",
            new Employee(
                Guid.NewGuid(),
                "フルネス太郎",
                "フルネスタロウ",
                new Department(Guid.NewGuid(), "販売管理部")));

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _employeeAccountRepository = new Mock<IEmployeeAccountRepository>();
        _passwordHasher = new Mock<IPasswordHasher>();
        _accessTokenGenerator = new Mock<IAccessTokenGenerator>();

        _accessTokenGenerator
            .Setup(g => g.Generate(It.IsAny<IEnumerable<Claim>>()))
            .Returns(DummyToken);

        _interactor = new EmployeeLoginInteractor(
            _employeeAccountRepository.Object,
            _passwordHasher.Object,
            _accessTokenGenerator.Object);
    }

    [TestMethod(DisplayName = "認証に成功するとアカウントとトークンを返す")]
    public async Task ExecuteAsync_ValidCredentials_ReturnsAccountAndToken()
    {
        var account = CreateAccount();
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync("fullness"))
            .ReturnsAsync(account);
        _passwordHasher
            .Setup(h => h.VerifyPassword(account.Password, "Password123"))
            .Returns(true);

        var result = await _interactor.ExecuteAsync(new EmployeeLoginParam("fullness", "Password123"));

        Assert.AreEqual(account, result.Account);
        Assert.AreEqual(DummyToken, result.Token);
    }

    [TestMethod(DisplayName = "トークンにはsub・name・employee_nameのクレームが含まれる")]
    public async Task ExecuteAsync_ValidCredentials_GeneratesExpectedClaims()
    {
        var account = CreateAccount();
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync("fullness"))
            .ReturnsAsync(account);
        _passwordHasher
            .Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        IEnumerable<Claim>? captured = null;
        _accessTokenGenerator
            .Setup(g => g.Generate(It.IsAny<IEnumerable<Claim>>()))
            .Callback<IEnumerable<Claim>>(c => captured = c.ToList())
            .Returns(DummyToken);

        await _interactor.ExecuteAsync(new EmployeeLoginParam("fullness", "Password123"));

        Assert.IsNotNull(captured);
        var claims = captured!.ToList();
        Assert.AreEqual(account.Id.ToString(), claims.Single(c => c.Type == "sub").Value);
        Assert.AreEqual("fullness", claims.Single(c => c.Type == "name").Value);
        Assert.AreEqual("フルネス太郎", claims.Single(c => c.Type == "employee_name").Value);
    }

    [TestMethod(DisplayName = "アカウントが存在しない場合はAuthenticationFailedExceptionをスローする")]
    public async Task ExecuteAsync_AccountNotFound_ThrowsAuthenticationFailedException()
    {
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync(It.IsAny<string>()))
            .ReturnsAsync((EmployeeAccount?)null);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(new EmployeeLoginParam("unknown", "Password123")));
    }

    [TestMethod(DisplayName = "アカウントが存在しない場合もパスワード照合を行い応答時間を揃える")]
    public async Task ExecuteAsync_AccountNotFound_VerifiesDummyPasswordHash()
    {
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync(It.IsAny<string>()))
            .ReturnsAsync((EmployeeAccount?)null);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(new EmployeeLoginParam("unknown", "Password123")));

        _passwordHasher.Verify(
            h => h.VerifyPassword(It.IsAny<string>(), "Password123"),
            Times.Once);
    }

    [TestMethod(DisplayName = "パスワードが一致しない場合はAuthenticationFailedExceptionをスローする")]
    public async Task ExecuteAsync_InvalidPassword_ThrowsAuthenticationFailedException()
    {
        var account = CreateAccount();
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync("fullness"))
            .ReturnsAsync(account);
        _passwordHasher
            .Setup(h => h.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(new EmployeeLoginParam("fullness", "wrong")));
    }

    [TestMethod(DisplayName = "認証に失敗した場合はトークンを生成しない")]
    public async Task ExecuteAsync_AuthenticationFailed_DoesNotGenerateToken()
    {
        _employeeAccountRepository
            .Setup(r => r.FindByAccountNameAsync(It.IsAny<string>()))
            .ReturnsAsync((EmployeeAccount?)null);

        await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
            () => _interactor.ExecuteAsync(new EmployeeLoginParam("unknown", "Password123")));

        _accessTokenGenerator.Verify(
            g => g.Generate(It.IsAny<IEnumerable<Claim>>()),
            Times.Never);
    }
}