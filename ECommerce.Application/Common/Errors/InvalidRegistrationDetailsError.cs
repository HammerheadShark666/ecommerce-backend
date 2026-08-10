using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class InvalidRegistrationDetailsError : ApiError
{
    public InvalidRegistrationDetailsError()
        : base(
            "Invalid registration details error",
            StatusCodes.Status400BadRequest)
    {
    }
}
