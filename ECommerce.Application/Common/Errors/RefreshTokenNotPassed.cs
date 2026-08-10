using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class RefreshTokenNotPassed : ApiError
{
    public RefreshTokenNotPassed()
        : base(
            "Refresh token not passed.",
            StatusCodes.Status400BadRequest)
    {
    }
}
