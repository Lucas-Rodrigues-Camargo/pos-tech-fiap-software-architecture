using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Mvc;
using OficinaMecanica.Application.Common;

namespace OficinaMecanica.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => NotFound(new { error = exception.Message }),
            UnauthorizedAccessException => Unauthorized(new { error = exception.Message }),
            ValidationException or InvalidOperationException or ArgumentException => BadRequest(new { error = exception.Message }),
            _ => Rethrow(exception)
        };
    }

    private static IActionResult Rethrow(Exception exception)
    {
        ExceptionDispatchInfo.Capture(exception).Throw();
        throw exception;
    }
}

