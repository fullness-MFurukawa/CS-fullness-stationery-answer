using System.Reflection;

using Microsoft.OpenApi;

namespace Backend.Api.Extensions;

/// <summary>
/// Swagger（OpenAPI）に関するDI登録とミドルウェア登録の拡張メソッド
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Swaggerのドキュメント生成をDIコンテナへ登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fullness Stationery データ管理サービス API",
                Version = "v1",
                Description = "総合開発演習 解答例。管理者向けのデータ管理サービス（UC009〜UC018）を提供する。"
            });

            // コントローラとViewModelのXMLコメントを取り込む
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
            if (File.Exists(xmlFilePath))
            {
                options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
            }

            // 認証方式（HttpOnly Cookie）をドキュメント上に明示する
            // Cookieはブラウザが自動送信するため、Swagger UIのAuthorize欄からは入力できない
            options.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Cookie,
                Name = "auth",
                Description = "ログインAPIの実行後、HttpOnly Cookie が自動的に付与される。"
            });
        });

        return services;
    }

    /// <summary>
    /// Swagger UIを有効にする
    /// </summary>
    /// <param name="app">アプリケーションビルダー</param>
    /// <returns>アプリケーションビルダー</returns>
    /// <remarks>
    /// 本演習では模範解答として動作確認できるよう有効化を許可する
    /// </remarks>
    public static WebApplication UseSwaggerUi(this WebApplication app)
    {
        var enabledInConfig = app.Configuration.GetValue<bool>("Swagger:Enabled");

        if (!app.Environment.IsDevelopment() && !enabledInConfig) return app;

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fullness Stationery API v1");
            options.DocumentTitle = "Fullness Stationery データ管理サービス API";
        });

        return app;
    }
}