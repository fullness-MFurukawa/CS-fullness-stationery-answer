using System.Text;
using Ec.Api.Extensions;
using Ec.Api.Middleware;
using Ec.Application.Extensions;
using Ec.Infrastructure.Extensions;
using Ec.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 各層のサービスをDIコンテナへ登録
// ------------------------------------------------------------
builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ------------------------------------------------------------
// 認証（JWTをAuthorizationヘッダーのBearerトークンから読み取る）
// ------------------------------------------------------------
// 管理サービス側とは別の署名鍵・発行者・対象者を用いる（CustomerJwtセクション）。
// これにより、顧客のトークンで管理サービスのAPIを呼び出せないようにする。
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var signingKey = jwtSection["SigningKey"]
    ?? throw new InvalidOperationException($"{JwtOptions.SectionName}:SigningKey が設定されていません。");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // "sub" などのクレーム名をそのまま扱う（既定ではURI形式へ変換される）
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        // トークンは Authorization: Bearer ヘッダーで受け取る。
        // フロントエンドは同一オリジンで動くため、CookieもCORSも不要。
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddApiBehavior();
builder.Services.AddProblemDetails();

// 組み込みのOpenAPIドキュメント生成（.NET 9以降）。UIは含まないためScalarで表示する
builder.Services.AddOpenApi();

var app = builder.Build();

// ------------------------------------------------------------
// ミドルウェアパイプライン
// ------------------------------------------------------------
// 例外処理は最も外側に配置し、後続のすべての例外を捕捉する
app.UseMiddleware<AppExceptionMiddleware>();

// OpenAPIのJSONドキュメントと、それを閲覧するScalarのUIを公開する。
// /openapi/v1.json にドキュメント、/scalar/v1 にUIが出る
app.MapOpenApi();
app.MapScalarApiReference();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
/// 統合テストから参照するためのエントリポイントの公開
/// </summary>
public partial class Program;