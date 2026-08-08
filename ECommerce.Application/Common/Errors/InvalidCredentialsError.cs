using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class InvalidCredentialsError : ApiError
{
    public InvalidCredentialsError()
        : base(
            "Invalid login credentials.",
            StatusCodes.Status401Unauthorized)
    {
    }
}
