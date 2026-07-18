using Ec.Application.Interactor;
using Ec.Application.Usecases;
using Microsoft.Extensions.DependencyInjection;
namespace Ec.Application.Extensions;

/// <summary>
/// アプリケーション層のサービスをDIコンテナへ登録する拡張メソッド
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// ユースケース（Interactor）を登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 顧客アカウント（UC001 / UC002）
        services.AddScoped<ICustomerRegisterUsecase, CustomerRegisterInteractor>();
        services.AddScoped<ICustomerLoginUsecase, CustomerLoginInteractor>();
        // 商品検索・詳細（UC003 / UC004）
        services.AddScoped<IProductSearchUsecase, ProductSearchInteractor>();
        services.AddScoped<IProductDetailUsecase, ProductDetailInteractor>();
        // 購入確定（UC005）
        services.AddScoped<IOrderCreateUsecase, OrderCreateInteractor>();
        // 購入履歴（UC007）
        services.AddScoped<IOrderHistorySearchUsecase, OrderHistorySearchInteractor>();
        services.AddScoped<IOrderDetailUsecase, OrderDetailInteractor>();
        // 画面の選択肢用（補助ユースケース）
        services.AddScoped<ICategorySearchUsecase, CategorySearchInteractor>();
        services.AddScoped<IPaymentMethodSearchUsecase, PaymentMethodSearchInteractor>();
        return services;
    }
}