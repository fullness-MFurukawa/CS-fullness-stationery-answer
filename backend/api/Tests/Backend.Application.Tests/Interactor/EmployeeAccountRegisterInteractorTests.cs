using Backend.Application.Exceptions;
using Backend.Application.Interactor;
using Backend.Application.Interfaces;
using Backend.Application.Params;
using Backend.Application.Tests.Fakes;
using Backend.Domain.Models;
using Backend.Domain.Repositories;

using Moq;

namespace Backend.Application.Tests.Interactor;

[TestClass]
[TestCategory("Backend.Application.Interactor")]
public class EmployeeAccountRegisterInteractorTests
{
    private const string PlainPassword = "Password123";
    private const string HashedPassword = "hashed-password";

    private Mock<IEmployeeAccountRepository> _employeeAccountRepository = null!;
    private Mock<IEmployeeRepository> _employeeRepository = null!;
    private Mock<IPasswordHasher> _passwordHasher = null!;
    private EmployeeAccountRegisterInteractor _interactor = null!;

    private Employee _employee = null!;

    /// <summary>
    /// テストごとにモックとテスト対象を初期化する
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        _employee = new Employee(
            Guid.NewGuid(),
            "鈴木花子",
            "スズキハナコ",
            new Department(Guid.NewGuid(), "商品企画部"));

        _employeeAccountRepository = new Mock<IEmployeeAccountRepository>();
        _employeeAccountRepository
            .Setup(r => r.ExistsByAccountNameAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _employeeAccountRepository
            .Setup(r => r.AddAsync(It.IsAny<EmployeeAccount>()))
            .Returns(Task.CompletedTask);

        _employeeRepository = new Mock<IEmployeeRepository>();
        _employeeRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_employee);

        _passwordHasher = new Mock<IPasswordHasher>();
        _passwordHasher
            .Setup(h => h.HashPassword(PlainPassword))
            .Returns(HashedPassword);

        _interactor = new EmployeeAccountRegisterInteractor(
            _employeeAccountRepository.Object,
            _employeeRepository.Object,
            _passwordHasher.Object,
            new PassThroughUnitOfWork());
    }

    /// <summary>
    /// テスト用の入力値を生成する
    /// </summary>
    private EmployeeAccountRegisterParam CreateParam(string accountName = "hanako01")
        => new(_employee.Id, accountName, PlainPassword);

    [TestMethod(DisplayName = "アカウントを登録し登録内容を返す")]
    public async Task ExecuteAsync_ValidParam_RegistersAndReturnsAccount()
    {
        var account = await _interactor.ExecuteAsync(CreateParam());

        Assert.AreEqual("hanako01", account.Name);
        Assert.AreEqual(HashedPassword, account.Password);
        Assert.AreEqual(_employee, account.Employee);
        Assert.AreNotEqual(Guid.Empty, account.Id);

        _employeeAccountRepository.Verify(
            r => r.AddAsync(It.Is<EmployeeAccount>(a => a.Name == "hanako01")),
            Times.Once);
    }

    [TestMethod(DisplayName = "パスワードはハッシュ化してから集約に設定される")]
    public async Task ExecuteAsync_ValidParam_StoresHashedPassword()
    {
        EmployeeAccount? saved = null;
        _employeeAccountRepository
            .Setup(r => r.AddAsync(It.IsAny<EmployeeAccount>()))
            .Callback<EmployeeAccount>(a => saved = a)
            .Returns(Task.CompletedTask);

        await _interactor.ExecuteAsync(CreateParam());

        Assert.IsNotNull(saved);
        Assert.AreEqual(HashedPassword, saved!.Password);
        Assert.AreNotEqual(PlainPassword, saved.Password);
        _passwordHasher.Verify(h => h.HashPassword(PlainPassword), Times.Once);
    }

    [TestMethod(DisplayName = "アカウント名が重複している場合はExistsExceptionをスローする")]
    public async Task ExecuteAsync_DuplicatedAccountName_ThrowsExistsException()
    {
        _employeeAccountRepository
            .Setup(r => r.ExistsByAccountNameAsync("fullness"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam("fullness")));
    }

    [TestMethod(DisplayName = "アカウント名が重複している場合は社員の取得も登録も行わない")]
    public async Task ExecuteAsync_DuplicatedAccountName_DoesNotProceed()
    {
        _employeeAccountRepository
            .Setup(r => r.ExistsByAccountNameAsync("fullness"))
            .ReturnsAsync(true);

        await Assert.ThrowsExactlyAsync<ExistsException>(
            () => _interactor.ExecuteAsync(CreateParam("fullness")));

        _employeeRepository.Verify(r => r.FindByIdAsync(It.IsAny<Guid>()), Times.Never);
        _employeeAccountRepository.Verify(r => r.AddAsync(It.IsAny<EmployeeAccount>()), Times.Never);
    }

    [TestMethod(DisplayName = "社員が存在しない場合はNotFoundExceptionをスローする")]
    public async Task ExecuteAsync_EmployeeNotFound_ThrowsNotFoundException()
    {
        _employeeRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));
    }

    [TestMethod(DisplayName = "社員が存在しない場合はハッシュ化も登録も行わない")]
    public async Task ExecuteAsync_EmployeeNotFound_DoesNotHashOrRegister()
    {
        _employeeRepository
            .Setup(r => r.FindByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsExactlyAsync<NotFoundException>(
            () => _interactor.ExecuteAsync(CreateParam()));

        _passwordHasher.Verify(h => h.HashPassword(It.IsAny<string>()), Times.Never);
        _employeeAccountRepository.Verify(r => r.AddAsync(It.IsAny<EmployeeAccount>()), Times.Never);
    }

    [TestMethod(DisplayName = "識別IDはユースケースで採番される")]
    public async Task ExecuteAsync_CalledTwice_GeneratesDifferentIds()
    {
        var first = await _interactor.ExecuteAsync(CreateParam("hanako01"));
        var second = await _interactor.ExecuteAsync(CreateParam("hanako02"));

        Assert.AreNotEqual(first.Id, second.Id);
    }
}