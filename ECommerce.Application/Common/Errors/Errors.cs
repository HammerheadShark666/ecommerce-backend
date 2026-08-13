
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;

public abstract class ApplicationError(string message, int statusCode) : Error(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundError(string message) : ApplicationError(message, StatusCodes.Status404NotFound);

public sealed class ConflictError(string message) : ApplicationError(message, StatusCodes.Status409Conflict);

public sealed class ForbiddenError(string message) : ApplicationError(message, StatusCodes.Status403Forbidden);

public sealed class ValidationError(string propertyName, string message) : Error(message)
{
    public string PropertyName { get; } = propertyName;
}
