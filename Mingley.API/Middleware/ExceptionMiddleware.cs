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
        { _logger.LogWarning("Business: {Msg}", ex.Message); await Write(ctx, HttpStatusCode.BadRequest, ex.Message); }
        catch (UnauthorizedAccessException ex)
        { _logger.LogWarning("Unauth: {Msg}", ex.Message); await Write(ctx, HttpStatusCode.Unauthorized, ex.Message); }
        catch (Exception ex)
        { _logger.LogError(ex, "Unhandled"); await Write(ctx, HttpStatusCode.InternalServerError, "An unexpected error occurred."); }
    }

    static async Task Write(HttpContext ctx, HttpStatusCode code, string msg)
    {
        ctx.Response.StatusCode  = (int)code;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(
            ApiResponse<object>.Fail(msg, (int)code),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
