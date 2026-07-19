using Microsoft.AspNetCore.Mvc;

namespace Ec.Api.Extensions;
/// <summary>
/// モデル検証エラーのレスポンスに関する拡張メソッド
/// </summary>
public static class ApiBehaviorExtensions
{
    /// <summary>
    /// モデル検証エラー(400)のレスポンス形式をDIコンテナへ登録する
    /// </summary>
    /// <param name="services">サービスコレクション</param>
    /// <returns>サービスコレクション</returns>
    /// <remarks>
    /// [ApiController]が自動生成する400のレスポンスは、titleが英語で
    /// traceIdの形式もAppExceptionMiddlewareと異なるため、両者を揃える。
    /// AddControllers()がInvalidModelStateResponseFactoryへ既定値を設定するため、
    /// このメソッドはAddControllers()より後に呼び出す。
    /// </remarks>
    public static IServiceCollection AddApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "入力内容に誤りがあります",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = context.HttpContext.Request.Path
                };

                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return services;
    }
}