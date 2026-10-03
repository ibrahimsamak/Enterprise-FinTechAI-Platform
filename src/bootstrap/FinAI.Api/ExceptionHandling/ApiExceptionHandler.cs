using FinAI.BuildingBlocks.Application;
using FinAI.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinAI.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(IProblemDetailsService problems) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        ProblemDetails? details = ex switch
        {
            ValidationException v => new ValidationProblemDetails(
                v.Errors.GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            },
            IdempotencyConflictException c => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Request already in progress.",
                Detail = c.Message
            },
            BadHttpRequestException b => new ProblemDetails
            {
                Status = b.StatusCode,
                Title = "The request body could not be read.",
                Detail = b.InnerException?.Message ?? b.Message
            },
            NotFoundException n => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found.",
                Detail = n.Message
            },
            ConflictException c => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict with current state.",
                Detail = c.Message
            },
            _ => null
        };
        if (details is null) return false;

        http.Response.StatusCode = details.Status!.Value;
        return await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = details,
            Exception = ex
        });
    }
}
