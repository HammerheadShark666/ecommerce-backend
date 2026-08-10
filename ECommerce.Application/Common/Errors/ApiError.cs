using ECommerce.Application.Constants;
using FluentResults;

namespace ECommerce.Application.Common.Errors;

//public abstract class ApiError(
//    string message,
//    int statusCode) : Error(message)
//{
//    public int StatusCode { get; } = statusCode;
//}

public abstract class ApiError : Error
{
    protected ApiError(string message, int statusCode)
        : base(message) => Metadata[ErrorMetadataKeys.StatusCode] = statusCode;
}
