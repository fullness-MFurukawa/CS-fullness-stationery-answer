using System.Text;
using Backend.Api.Authentication;
using Backend.Api.Extensions;
using Backend.Api.Middleware;
using Backend.Application.Extensions;
using Backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 各層のサービスをDIコンテナへ登録
// ------------------------------------------------------------
builder.Services.Configure<AuthCookieOptions>(
    builder.Configuration.GetSection(AuthCookieOptions.SectionName));
builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ------------------------------------------------------------
// 認証（JWTをHttpOnly Cookieから読み取る）
// ------------------------------------------------------------
// ------------------------------------------------------------
// 認証（JWTをHttpOnly Cookieから読み取る）
// ------------------------------------------------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey が設定されていません。");

var authCookieName = builder.Configuration[$"{AuthCookieOptions.SectionName}:Name"] ?? "auth";

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
            NameClaimType = "name"
        };

        // Authorization ヘッダではなくCookieからトークンを取得する
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies[authCookieName];
                return Task.CompletedTask;
            }
        };
    });
    
builder.Services.AddAuthorization();

// ------------------------------------------------------------
// CORS（SPAからCookieを送信するため AllowCredentials が必須）
// ------------------------------------------------------------
const string CorsPolicyName = "SpaPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ------------------------------------------------------------
// マルチパート（商品画像アップロード）の上限
// ------------------------------------------------------------
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSwagger();

var app = builder.Build();

// ------------------------------------------------------------
// ミドルウェアパイプライン
// ------------------------------------------------------------

// 例外処理は最も外側に配置し、後続のすべての例外を捕捉する
app.UseMiddleware<AppExceptionMiddleware>();

app.UseSwaggerUi();


// アップロードされた商品画像を配信する
app.UseStaticFiles();

app.UseCors(CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// 統合テストから参照するためのエントリポイントの公開
/// </summary>
public partial class Program;