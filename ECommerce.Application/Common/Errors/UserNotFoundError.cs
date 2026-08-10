using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class UserNotFoundError : ApiError
{
    public UserNotFoundError()
        : base(
            "User not found.",
            StatusCodes.Status404NotFound)
    {
    }
}
