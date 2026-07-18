using Ec.Application.Interfaces;
using Ec.Domain.Repositories;
using Ec.Infrastructure.Adapters;
using Ec.Infrastructure.Contexts;
using Ec.Infrastructure.Factories;
using Ec.Infrastructure.Repositories;
using Ec.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Ec.Infrastructure.Extensions;

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
        services.AddSingleton<ProductCategoryAdapter>();
        services.AddSingleton<ProductStockAdapter>();
        services.AddSingleton<ProductAdapter>();
        services.AddSingleton<CustomerAdapter>();
        services.AddSingleton<OrderStatusAdapter>();
        services.AddSingleton<PaymentMethodAdapter>();
        services.AddSingleton<OrderDetailAdapter>();
        services.AddSingleton<OrderAdapter>();

        // ファクトリ（アダプタのみに依存し状態を持たないためシングルトン）
        services.AddSingleton<ProductFactory>();
        services.AddSingleton<OrderFactory>();

        // リポジトリ（DbContextに依存するためスコープ）
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();

        // ユニットオブワーク（DbContextに依存するためスコープ）
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // パスワードハッシュ（状態を持たないためシングルトン）
        services.AddSingleton<IPasswordHasher, Security.PasswordHasher>();

        // JWTの構成をバインドし、トークン生成を登録する（状態を持たないためシングルトン）
        // JwtOptions.SectionName は "CustomerJwt"。管理サービス側とは別の鍵を使う
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        return services;
    }
}