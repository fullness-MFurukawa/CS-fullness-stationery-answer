using Backend.Api.Adapters;
using Backend.Api.Authentication;

namespace Backend.Api.Extensions;

/// <summary>
/// API層のサービスをDIコンテナへ登録する拡張メソッド
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ViewModelとドメインオブジェクトを変換するアダプタを登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // リクエスト → ユースケースの入力値
        services.AddSingleton<LoginRequestAdapter>();
        services.AddSingleton<EmployeeAccountRegisterRequestAdapter>();
        services.AddSingleton<ProductRegisterRequestAdapter>();
        services.AddSingleton<ProductUpdateRequestAdapter>();
        services.AddSingleton<OrderStatusUpdateRequestAdapter>();

        // ドメインオブジェクト → レスポンス
        services.AddSingleton<LoginResponseAdapter>();
        services.AddSingleton<EmployeeResponseAdapter>();
        services.AddSingleton<EmployeeAccountResponseAdapter>();
        services.AddSingleton<CategoryResponseAdapter>();
        services.AddSingleton<ProductResponseAdapter>();
        services.AddSingleton<OrderDetailResponseAdapter>();
        services.AddSingleton<OrderResponseAdapter>();
        services.AddSingleton<OrderStatusResponseAdapter>();

        services.AddSingleton<AuthCookie>();
        services.AddSingleton<DashboardSummaryResponseAdapter>();

        return services;
    }
}