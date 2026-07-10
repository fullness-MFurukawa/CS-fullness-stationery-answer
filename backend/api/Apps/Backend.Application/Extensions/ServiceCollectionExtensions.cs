using Backend.Application.Interactor;
using Backend.Application.Usecases;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application.Extensions;

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
        // 担当者アカウント（UC009 / UC017）
        services.AddScoped<IEmployeeAccountRegisterUsecase, EmployeeAccountRegisterInteractor>();
        services.AddScoped<IEmployeeLoginUsecase, EmployeeLoginInteractor>();

        // 商品（UC010 / UC011 / UC012 / UC013）
        services.AddScoped<IProductRegisterUsecase, ProductRegisterInteractor>();
        services.AddScoped<IProductSearchUsecase, ProductSearchInteractor>();
        services.AddScoped<IProductUpdateUsecase, ProductUpdateInteractor>();
        services.AddScoped<IProductDeleteUsecase, ProductDeleteInteractor>();

        // 商品カテゴリ（UC014）
        services.AddScoped<ICategoryRegisterUsecase, CategoryRegisterInteractor>();

        // 注文（UC015 / UC016）
        services.AddScoped<IOrderHistorySearchUsecase, OrderHistorySearchInteractor>();
        services.AddScoped<IOrderStatusUpdateUsecase, OrderStatusUpdateInteractor>();

        // 画面の選択肢用（補助ユースケース）
        services.AddScoped<ICategorySearchUsecase, CategorySearchInteractor>();
        services.AddScoped<IEmployeeWithoutAccountSearchUsecase, EmployeeWithoutAccountSearchInteractor>();
        services.AddScoped<IOrderStatusSearchUsecase, OrderStatusSearchInteractor>();

        // 画像アップロード（UC018）
        services.AddScoped<IImageUploadUsecase, ImageUploadInteractor>();

        return services;
    }
}