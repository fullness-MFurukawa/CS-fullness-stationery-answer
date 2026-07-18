using Ec.Domain.Exceptions;
using Ec.Domain.Models;
namespace Ec.Domain.Tests.Models;

[TestClass]
[TestCategory("Ec.Domain.Models")]
public class CustomerTests
{
    /// <summary>
    /// テスト用の顧客を生成する（各項目は任意で上書き可能）
    /// </summary>
    private static Customer CreateCustomer(
        Guid? id = null,
        string name = "テスト顧客",
        string nameKana = "テストコキャク",
        string address1 = "東京都新宿区",
        string? address2 = "テストビル101",
        string phoneNumber = "090-1234-5678",
        string mailAddress = "test@example.com",
        string username = "testuser",
        string password = "hashed-password",
        DateTime? createdAt = null)
        => new(
            id ?? Guid.NewGuid(),
            name,
            nameKana,
            address1,
            address2,
            phoneNumber,
            mailAddress,
            username,
            password,
            createdAt ?? DateTime.Now);

    [TestMethod(DisplayName = "正しい値で生成でき各プロパティに設定される")]
    public void Constructor_ValidValues_SetsProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0);

        var customer = CreateCustomer(id: id, createdAt: createdAt);

        Assert.AreEqual(id, customer.Id);
        Assert.AreEqual("テスト顧客", customer.Name);
        Assert.AreEqual("テストコキャク", customer.NameKana);
        Assert.AreEqual("東京都新宿区", customer.Address1);
        Assert.AreEqual("テストビル101", customer.Address2);
        Assert.AreEqual("090-1234-5678", customer.PhoneNumber);
        Assert.AreEqual("test@example.com", customer.MailAddress);
        Assert.AreEqual("testuser", customer.Username);
        Assert.AreEqual("hashed-password", customer.Password);
        Assert.AreEqual(createdAt, customer.CreatedAt);
    }

    [TestMethod(DisplayName = "住所2は未指定でも生成できる")]
    public void Constructor_NullOptionalFields_IsAllowed()
    {
        var customer = CreateCustomer(address2: null);

        Assert.IsNull(customer.Address2);
    }

    [TestMethod(DisplayName = "顧客名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingName_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(name: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(name: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(name: "   "));
    }

    [TestMethod(DisplayName = "顧客名カナが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingNameKana_ThrowsDomainException()
    {
        // データベースの定義ではNULLを許容しているが、
        // 画面仕様(FP003)では必須のため、ドメインでも必須として扱う
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(nameKana: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(nameKana: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(nameKana: "   "));
    }

    [TestMethod(DisplayName = "住所1が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingAddress1_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(address1: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(address1: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(address1: "   "));
    }

    [TestMethod(DisplayName = "電話番号が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingPhoneNumber_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(phoneNumber: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(phoneNumber: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(phoneNumber: "   "));
    }

    [TestMethod(DisplayName = "メールアドレスが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingMailAddress_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "   "));
    }

    [TestMethod(DisplayName = "メールアドレスの形式が不正ならDomainExceptionをスローする")]
    public void Constructor_InvalidMailAddress_ThrowsDomainException()
    {
        // アットマークがない
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "testexample.com"));
        // ローカル部がない
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "@example.com"));
        // ドメイン部がない
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "test@"));
        // ドメイン部にドットがない
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "test@example"));
        // 空白を含む
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "test @example.com"));
        // アットマークが複数ある
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(mailAddress: "test@example@com"));
    }

    [TestMethod(DisplayName = "メールアドレスの形式が正しければ生成できる")]
    public void Constructor_ValidMailAddress_IsAllowed()
    {
        // サブドメインを含む
        Assert.AreEqual(
            "test@mail.example.co.jp",
            CreateCustomer(mailAddress: "test@mail.example.co.jp").MailAddress);
        // ローカル部にドットやプラスを含む
        Assert.AreEqual(
            "first.last+tag@example.com",
            CreateCustomer(mailAddress: "first.last+tag@example.com").MailAddress);
    }

    [TestMethod(DisplayName = "アカウント名が未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingUsername_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(username: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(username: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(username: "   "));
    }

    [TestMethod(DisplayName = "パスワードが未指定ならDomainExceptionをスローする")]
    public void Constructor_MissingPassword_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(password: null!));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(password: ""));
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(password: "   "));
    }

    [TestMethod(DisplayName = "識別子が空GUIDならDomainExceptionをスローする")]
    public void Constructor_EmptyGuid_ThrowsDomainException()
    {
        Assert.ThrowsExactly<DomainException>(() => CreateCustomer(id: Guid.Empty));
    }
}