using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class RefreshTokenNotFound : ApiError
{
    public RefreshTokenNotFound()
        : base(
            "Refresh token not found.",
            StatusCodes.Status404NotFound)
    {
    }
}
