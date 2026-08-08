using FluentResults;

namespace ECommerce.Application.Common.Errors;

public abstract class ApiError(
    string message,
    int statusCode) : Error(message)
{
    public int StatusCode { get; } = statusCode;
}
