using Ec.Domain.Models;
namespace Ec.Application.Results;

/// <summary>
/// UC002:顧客ログインの実行結果
/// </summary>
/// <param name="Customer">認証に成功した顧客</param>
/// <param name="Token">発行されたアクセストークン</param>
public sealed record CustomerLoginResult(
    Customer Customer,
    AccessToken Token);