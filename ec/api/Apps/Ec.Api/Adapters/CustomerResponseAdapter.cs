using Ec.Api.ViewModels.Responses;
using Ec.Domain.Adapters;
using Ec.Domain.Models;
namespace Ec.Api.Adapters;

/// <summary>
/// 顧客とレスポンスを変換するアダプタ
/// </summary>
/// <remarks>
/// レスポンスはパスワードを含まず復元もしないため、逆方向は未サポートとする。
/// </remarks>
public class CustomerResponseAdapter : IEntityAdapter<CustomerResponse, Customer>
{
    /// <summary>
    /// レスポンスから顧客へ変換する（未サポート）
    /// </summary>
    /// <param name="source">顧客のレスポンス</param>
    /// <returns>常に例外をスローする</returns>
    /// <exception cref="NotSupportedException">顧客を復元できないため</exception>
    public Customer ToDomain(CustomerResponse source)
        => throw new NotSupportedException("レスポンスから顧客への復元は行えません。");

    /// <summary>
    /// 顧客からレスポンスへ変換する
    /// </summary>
    /// <param name="domain">顧客</param>
    /// <returns>顧客のレスポンス</returns>
    public CustomerResponse ToSource(Customer domain)
        => new(domain.Id, domain.Name, domain.MailAddress, domain.Username);
}