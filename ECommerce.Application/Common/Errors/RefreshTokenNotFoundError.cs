using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class RefreshTokenNotFoundError : ApiError
{
    public RefreshTokenNotFoundError()
        : base(
            "Refresh token not found.",
            StatusCodes.Status404NotFound)
    {
    }
}
