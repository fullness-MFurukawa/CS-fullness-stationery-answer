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
                document.Info.Title = "Fullness Stationery EC API";
                document.Info.Version = "v1";
                document.Info.Description =
                    """
                    文具・雑貨販売ECサイト **Fullness Stationery** の顧客向けAPIです。🛍️

                    ## 📖 概要

                    このAPIは、顧客がECサイトを利用するための機能を提供します。
                    ユースケース UC001〜UC007 に対応します。

                    ## 🔑 認証

                    一部のAPIはログインが必要です。`POST /api/ec/auth/login` で
                    アクセストークンを取得し、画面右上の **Authentication** に設定してください。

                    | 認証 | エンドポイント |
                    | --- | --- |
                    | 🟢 不要 | 顧客登録・ログイン・商品検索・商品詳細・カテゴリ一覧・支払い方法一覧 |
                    | 🔒 必要 | 購入確定・購入履歴 |

                    ## ⚠️ 注意事項

                    - 🛒 カートはフロントエンドで保持し、購入確定（UC005）で初めて注文になります。
                    - 📦 在庫は購入確定時に悲観的ロックを行い、同時購入による在庫の不整合を防ぎます。
                    """;

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