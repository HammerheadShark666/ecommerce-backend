using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class RefreshTokenNotPassedError : ApiError
{
    public RefreshTokenNotPassedError()
        : base(
            "Refresh token not passed.",
            StatusCodes.Status400BadRequest)
    {
    }
}
