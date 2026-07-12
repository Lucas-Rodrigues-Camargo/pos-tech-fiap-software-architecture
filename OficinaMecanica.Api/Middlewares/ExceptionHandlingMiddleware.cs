using System.Net;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware>? logger = null)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (ArgumentException exception)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteAsync(context, HttpStatusCode.NotFound, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteAsync(context, HttpStatusCode.Unauthorized, exception.Message);
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "Unhandled request exception");
            await WriteAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno.");
        }
    }

    private static async Task WriteAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}
