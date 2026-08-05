using System.Net;
using System.Text.Json;
using Mingley.Application.DTOs.Common;

namespace Mingley.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (InvalidOperationException ex)
        {
            // These are business-logic exceptions we throw ourselves with a
            // deliberately user-facing message (e.g. "Insufficient coins...",
            // "Plan not found..."). Safe to show as-is — no inner exception,
            // no stack trace, no DB error text.
            _logger.LogWarning(ex, "Business: {Msg}", ex.Message);
            await Write(ctx, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauth: {Msg}", ex.Message);
            await Write(ctx, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            // Anything unexpected (DB errors, null refs, third-party API failures, etc).
            // Full details go to the server log only — the client gets one clean,
            // generic message plus a correlation id to quote when reporting it.
            var correlationId = ctx.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            await Write(ctx, HttpStatusCode.InternalServerError,
                $"Something went wrong on our end. Please try again in a moment. (ref: {correlationId})");
        }
    }

    static async Task Write(HttpContext ctx, HttpStatusCode code, string msg)
    {
        ctx.Response.StatusCode = (int)code;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(
            ApiResponse<object>.Fail(msg, (int)code),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}