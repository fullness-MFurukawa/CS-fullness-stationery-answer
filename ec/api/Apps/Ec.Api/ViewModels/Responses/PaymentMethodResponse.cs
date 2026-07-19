namespace Ec.Api.ViewModels.Responses;

/// <summary>
/// 支払い方法のレスポンス
/// </summary>
/// <param name="PaymentMethodId">支払い方法識別ID</param>
/// <param name="Name">支払い方法名</param>
public sealed record PaymentMethodResponse(
    int PaymentMethodId,
    string Name);