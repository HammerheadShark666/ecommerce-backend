using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class UserNotFound : ApiError
{
    public UserNotFound()
        : base(
            "User not found.",
            StatusCodes.Status404NotFound)
    {
    }
}
