using Ec.Domain.Models;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class CustomerRepositoryTests : RepositoryTestBase
{
    private CustomerRepository _repository = null!;

    /// <summary>
    /// テスト対象のリポジトリを初期化する
    /// </summary>
    [TestInitialize]
    public void SetUpRepository()
    {
        _repository = new CustomerRepository(Context, new CustomerAdapter());
    }

    /// <summary>
    /// テスト用の顧客を生成する（メールとアカウント名は一意にする）
    /// </summary>
    private static Customer CreateCustomer(string suffix)
        => new(
            Guid.NewGuid(),
            "テスト顧客",
            "テストコキャク",
            "東京都新宿区",
            "テストビル101",
            "090-1234-5678",
            $"test_{suffix}@example.com",
            $"user_{suffix}",
            "hashed-password",
            DateTime.Now);

    [TestMethod(DisplayName = "顧客を登録し、識別IDで取得できる")]
    public async Task AddAsync_ThenFindById_ReturnsCustomer()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = CreateCustomer(suffix);

        await _repository.AddAsync(customer);
        var found = await _repository.FindByIdAsync(customer.Id);

        Assert.IsNotNull(found);
        Assert.AreEqual(customer.Id, found!.Id);
        Assert.AreEqual($"test_{suffix}@example.com", found.MailAddress);
        Assert.AreEqual($"user_{suffix}", found.Username);
    }

    [TestMethod(DisplayName = "メールアドレスで顧客を取得できる")]
    public async Task FindByMailAddress_ReturnsCustomer()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var customer = CreateCustomer(suffix);
        await _repository.AddAsync(customer);

        var found = await _repository.FindByMailAddressAsync($"test_{suffix}@example.com");

        Assert.IsNotNull(found);
        Assert.AreEqual(customer.Id, found!.Id);
    }

    [TestMethod(DisplayName = "存在しないメールアドレスならnullを返す")]
    public async Task FindByMailAddress_NotFound_ReturnsNull()
    {
        var found = await _repository.FindByMailAddressAsync("notexist@example.com");

        Assert.IsNull(found);
    }

    [TestMethod(DisplayName = "存在しない識別IDならnullを返す")]
    public async Task FindById_NotFound_ReturnsNull()
    {
        var found = await _repository.FindByIdAsync(Guid.NewGuid());

        Assert.IsNull(found);
    }

    [TestMethod(DisplayName = "登録済みのメールアドレスはExistsByMailAddressがtrueを返す")]
    public async Task ExistsByMailAddress_Registered_ReturnsTrue()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await _repository.AddAsync(CreateCustomer(suffix));

        var exists = await _repository.ExistsByMailAddressAsync($"test_{suffix}@example.com");

        Assert.IsTrue(exists);
    }

    [TestMethod(DisplayName = "未登録のメールアドレスはExistsByMailAddressがfalseを返す")]
    public async Task ExistsByMailAddress_NotRegistered_ReturnsFalse()
    {
        var exists = await _repository.ExistsByMailAddressAsync("notexist@example.com");

        Assert.IsFalse(exists);
    }

    [TestMethod(DisplayName = "登録済みのアカウント名はExistsByUsernameがtrueを返す")]
    public async Task ExistsByUsername_Registered_ReturnsTrue()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        await _repository.AddAsync(CreateCustomer(suffix));

        var exists = await _repository.ExistsByUsernameAsync($"user_{suffix}");

        Assert.IsTrue(exists);
    }

    [TestMethod(DisplayName = "未登録のアカウント名はExistsByUsernameがfalseを返す")]
    public async Task ExistsByUsername_NotRegistered_ReturnsFalse()
    {
        var exists = await _repository.ExistsByUsernameAsync("notexistuser");

        Assert.IsFalse(exists);
    }
}