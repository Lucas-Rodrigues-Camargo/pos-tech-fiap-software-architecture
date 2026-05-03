using System.Net;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
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
    }

    private static async Task WriteAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = message });
    }
}
