using Microsoft.AspNetCore.Http;
using FluentResults;

namespace ECommerce.Application.Common.Errors;

public sealed class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) => Metadata.Add("StatusCode", StatusCodes.Status404NotFound);
}

