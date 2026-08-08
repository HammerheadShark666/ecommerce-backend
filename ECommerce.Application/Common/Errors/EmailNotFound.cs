using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;
 
public sealed class EmailNotFound : ApiError
{
    public EmailNotFound()
        : base(
            "Email not found.",
            StatusCodes.Status404NotFound)
    {
    }
}
