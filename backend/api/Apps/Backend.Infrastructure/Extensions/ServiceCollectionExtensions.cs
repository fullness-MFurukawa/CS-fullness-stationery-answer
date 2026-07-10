using Backend.Application.Interfaces;
using Backend.Domain.Repositories;
using Backend.Infrastructure.Adapters;
using Backend.Infrastructure.Contexts;
using Backend.Infrastructure.Factories;
using Backend.Infrastructure.Repositories;
using Backend.Infrastructure.Security;
using Backend.Infrastructure.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure.Extensions;

/// <summary>
/// インフラストラクチャ層のサービスをDIコンテナへ登録する拡張メソッド
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// DbContext・アダプタ・ファクトリ・リポジトリ・ユニットオブワークを登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <param name="configuration">アプリケーション構成</param>
    /// <returns>サービスコレクション</returns>
    /// <exception cref="InvalidOperationException">接続文字列が設定されていない場合</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FullnessEc")
            ?? throw new InvalidOperationException("接続文字列 'FullnessEc' が設定されていません。");

        // データベースコンテキスト（スコープ）
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // アダプタ（状態を持たないためシングルトン）
        services.AddSingleton<DepartmentAdapter>();
        services.AddSingleton<EmployeeAdapter>();
        services.AddSingleton<EmployeeAccountAdapter>();
        services.AddSingleton<ProductCategoryAdapter>();
        services.AddSingleton<ProductStockAdapter>();
        services.AddSingleton<ProductAdapter>();
        services.AddSingleton<CustomerAdapter>();
        services.AddSingleton<OrderStatusAdapter>();
        services.AddSingleton<PaymentMethodAdapter>();
        services.AddSingleton<OrderDetailAdapter>();
        services.AddSingleton<OrderAdapter>();

        // ファクトリ（アダプタのみに依存し状態を持たないためシングルトン）
        services.AddSingleton<EmployeeFactory>();
        services.AddSingleton<EmployeeAccountFactory>();
        services.AddSingleton<ProductFactory>();
        services.AddSingleton<OrderFactory>();

        // リポジトリ（DbContextに依存するためスコープ）
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeAccountRepository, EmployeeAccountRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();

        // ユニットオブワーク（DbContextに依存するためスコープ）
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // パスワードハッシュ（状態を持たないためシングルトン）
        services.AddSingleton<IPasswordHasher, Backend.Infrastructure.Security.PasswordHasher>();
        // JWTの構成をバインドし、トークン生成を登録する（状態を持たないためシングルトン）
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        // 画像保存の設定をバインドし、ローカルファイルシステムへの画像保存を登録する（状態を持たないためシングルトン）
        services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
        services.AddScoped<IImageStorage, LocalImageStorage>();

        return services;
    }
}