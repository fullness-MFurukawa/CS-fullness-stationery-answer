using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Repositories;
namespace Ec.Infrastructure.Tests.Repositories;

[TestClass]
[TestCategory("Ec.Infrastructure.Repositories")]
public class PaymentMethodRepositoryTests : RepositoryTestBase
{
    private PaymentMethodRepository CreateRepository()
        => new(Context, new PaymentMethodAdapter());

    [TestMethod(DisplayName = "すべての支払い方法を取得できる")]
    public async Task FindAllAsync_ReturnsPaymentMethods()
    {
        var repository = CreateRepository();

        var methods = await repository.FindAllAsync();

        Assert.IsNotEmpty(methods);
        // UC005時点では「現金」が登録されている
        Assert.IsTrue(methods.Any(m => m.Name == "現金"));
    }

    [TestMethod(DisplayName = "IDを指定して支払い方法を取得できる")]
    public async Task FindByIdAsync_ExistingId_ReturnsPaymentMethod()
    {
        var repository = CreateRepository();
        var expected = (await repository.FindAllAsync()).First(m => m.Name == "現金");

        var actual = await repository.FindByIdAsync(expected.Id);

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Id, actual!.Id);
        Assert.AreEqual("現金", actual.Name);
    }

    [TestMethod(DisplayName = "存在しないIDを指定するとnullを返す")]
    public async Task FindByIdAsync_NotExistingId_ReturnsNull()
    {
        var repository = CreateRepository();

        var actual = await repository.FindByIdAsync(9999);

        Assert.IsNull(actual);
    }
}