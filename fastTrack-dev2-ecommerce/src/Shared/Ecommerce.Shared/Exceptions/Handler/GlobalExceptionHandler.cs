using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.Exceptions.Handler;

// Turns exceptions into RFC 7807 ProblemDetails. Unexpected errors never leak their message.
public class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> log)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            BadRequestException => StatusCodes.Status400BadRequest,
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest, // Ordering value objects (Address.Of, OrderName.Of...)
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        var errorId = Guid.NewGuid().ToString("N")[..8];
        if (status == StatusCodes.Status500InternalServerError)
            log.LogError(exception, "Unhandled error {ErrorId}", errorId);
        else
            log.LogInformation("Request failed with {Status}: {Message}", status, exception.Message);

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails =
            {
                Status = status,
                Title = status == StatusCodes.Status500InternalServerError ? "An unexpected error occurred." : exception.Message,
                Extensions = { ["errorId"] = errorId },
            },
        });
    }
}
