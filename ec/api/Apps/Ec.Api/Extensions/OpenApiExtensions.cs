using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
namespace Ec.Api.Extensions;

/// <summary>
/// 組み込みOpenAPIの構成に関する拡張メソッド
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Bearer認証のセキュリティ定義を含むOpenAPIドキュメントの生成を登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    /// <remarks>
    /// この定義があると、Scalar等のUIで「Authentication」欄からトークンを設定でき、
    /// 各リクエストのAuthorizationヘッダーへ自動で付与される。
    /// </remarks>
    public static IServiceCollection AddEcOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Bearerトークン（JWT）のセキュリティスキームを定義する
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "ログインAPIで取得したアクセストークンを入力してください。",
                };

                // すべての操作に既定でBearerを要求する定義を付ける。
                // [AllowAnonymous] のエンドポイントもUI上は入力欄が出るが、
                // トークンなしでも呼べるため実害はない。
                var requirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                };
                foreach (var path in document.Paths.Values)
                {
                    foreach (var operation in path.Operations!.Values)
                    {
                        operation.Security ??= [];
                        operation.Security.Add(requirement);
                    }
                }
                return Task.CompletedTask;
            });
        });
        return services;
    }
}