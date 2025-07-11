using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using OM.Application.Exceptions;
using OM.Domain.Exceptions;

namespace OM.Api.Filters;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;

    public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
    {
        var exception = context.Exception;

        switch (exception)
        {
            case BadRequestException validationException:
                HandleBadRequestException(context, validationException);
                break;



            case ValidationException validationException:
                HandleValidationException(context, validationException);
                break;

            case ArgumentException argumentException:
                HandleArgumentException(context, argumentException);
                break;

            case InvalidOperationException invalidOperationException:
                HandleInvalidOperationException(context, invalidOperationException);
                break;

            default:
                HandleGenericException(context, exception);
                break;
        }
    }

    private void HandleBadRequestException(ExceptionContext context, BadRequestException validationException)
    {
        _logger.LogWarning(validationException, "Bad request error occurred");
        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = validationException.Message,
            Instance = context.HttpContext.Request.Path
        };
        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;

    }

    private void HandleValidationException(ExceptionContext context, ValidationException exception)
    {
        _logger.LogWarning(exception, "Validation error occurred");

        var details = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleArgumentException(ExceptionContext context, ArgumentException exception)
    {
        _logger.LogWarning(exception, "Argument error occurred");

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleInvalidOperationException(ExceptionContext context, InvalidOperationException exception)
    {
        _logger.LogWarning(exception, "Invalid operation error occurred");

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = exception.Message,
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private void HandleGenericException(ExceptionContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An error occurred while processing your request.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;
    }
}
