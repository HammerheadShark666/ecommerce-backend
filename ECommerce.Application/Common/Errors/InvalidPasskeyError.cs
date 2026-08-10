using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;

public sealed class InvalidPasskeyError : ApiError
{
    public InvalidPasskeyError()
        : base("Invalid or expired code. Please try again.",
               StatusCodes.Status400BadRequest)
    {
    }
}
