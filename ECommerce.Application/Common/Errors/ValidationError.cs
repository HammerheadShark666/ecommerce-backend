using Microsoft.AspNetCore.Http;
using FluentResults;

namespace ECommerce.Application.Common.Errors;

public sealed class ValidationError : Error
{
    public ValidationError(string message) : base(message) => Metadata.Add("StatusCode", StatusCodes.Status400BadRequest);
}
