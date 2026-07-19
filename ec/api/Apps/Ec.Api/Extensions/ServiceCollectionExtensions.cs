using Ec.Api.Adapters;
namespace Ec.Api.Extensions;

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
        //services.AddSingleton<CustomerRegisterRequestAdapter>();
        services.AddSingleton<LoginRequestAdapter>();
        //services.AddSingleton<OrderCreateRequestAdapter>();

        // ドメインオブジェクト → レスポンス
        services.AddSingleton<LoginResponseAdapter>();
        //services.AddSingleton<CustomerResponseAdapter>();
        //services.AddSingleton<CategoryResponseAdapter>();
        //services.AddSingleton<ProductResponseAdapter>();
        //services.AddSingleton<PaymentMethodResponseAdapter>();
        //services.AddSingleton<OrderDetailResponseAdapter>();
        //services.AddSingleton<OrderResponseAdapter>();

        return services;
    }
}