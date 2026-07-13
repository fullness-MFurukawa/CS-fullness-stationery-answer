using Backend.Application.Exceptions;
using Backend.Domain.Exceptions;
using Backend.Infrastructure.Exceptions;

using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Middleware;

/// <summary>
/// パイプライン全体の例外を捕捉し、ProblemDetails形式のレスポンスへ変換するミドルウェア
/// </summary>
public class AppExceptionMiddleware
{
    /// <summary>
    /// 技術的障害の際にクライアントへ返す固定メッセージ
    /// 内部情報を推測させないため、例外のメッセージは返さない
    /// </summary>
    private const string InternalErrorMessage = "システムエラーが発生しました。管理者に連絡してください。";

    private readonly RequestDelegate _next;
    private readonly ILogger<AppExceptionMiddleware> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="next">次のミドルウェア</param>
    /// <param name="logger">ロガー</param>
    /// <param name="problemDetailsService">ProblemDetailsの生成サービス</param>
    public AppExceptionMiddleware(
        RequestDelegate next,
        ILogger<AppExceptionMiddleware> logger,
        IProblemDetailsService problemDetailsService)
    {
        _next = next;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    /// <summary>
    /// リクエストを処理し、発生した例外を捕捉する
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    /// <summary>
    /// 例外をレスポンスへ変換する
    /// </summary>
    /// <param name="context">HTTPコンテキスト</param>
    /// <param name="exception">発生した例外</param>
    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        // 既にレスポンスの送信が始まっている場合は書き換えられない
        if (context.Response.HasStarted)
        {
            _logger.LogCritical(exception, "レスポンス送信中に例外が発生しました。");
            throw exception;
        }

        var (statusCode, title, detail) = Resolve(exception);

        WriteLog(exception);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            }
        });
    }

    /// <summary>
    /// 例外の種類からステータスコードと応答内容を決定する
    /// </summary>
    /// <param name="exception">発生した例外</param>
    /// <returns>ステータスコード・タイトル・詳細</returns>
    private static (int StatusCode, string Title, string Detail) Resolve(Exception exception)
        => exception switch
        {
            // 業務ルール違反（ドメインの不変条件）
            DomainException ex
                => (StatusCodes.Status400BadRequest, "入力内容に誤りがあります", ex.Message),

            // 認証失敗（原因は区別しない）
            AuthenticationFailedException ex
                => (StatusCodes.Status401Unauthorized, "認証に失敗しました", ex.Message),

            // 対象が存在しない
            NotFoundException ex
                => (StatusCodes.Status404NotFound, "対象が見つかりません", ex.Message),

            // 重複登録
            ExistsException ex
                => (StatusCodes.Status409Conflict, "既に登録されています", ex.Message),

            // 想定内の技術的障害（DB接続失敗など）。内部情報は返さない
            InternalException
                => (StatusCodes.Status500InternalServerError, "システムエラー", InternalErrorMessage),

            // 想定外の例外。内部情報は返さない
            _ => (StatusCodes.Status500InternalServerError, "システムエラー", InternalErrorMessage)
        };

    /// <summary>
    /// 例外の種類に応じてログを出力する
    /// </summary>
    /// <param name="exception">発生した例外</param>
    private void WriteLog(Exception exception)
    {
        switch (exception)
        {
            // 業務エラーは想定内のためWarningに留め、ログを汚さない
            case DomainException:
            case AuthenticationFailedException:
            case NotFoundException:
            case ExistsException:
                _logger.LogWarning("業務エラーが発生しました。{Message}", exception.Message);
                break;

            // 想定内のインフラ障害。スタックトレースと内部例外まで含めて記録する
            case InternalException:
                _logger.LogError(exception, "インフラストラクチャで障害が発生しました。");
                break;

            // 想定外＝不具合の可能性が高いため、最も高いレベルで記録する
            default:
                _logger.LogCritical(exception, "想定外の例外が発生しました。");
                break;
        }
    }
}