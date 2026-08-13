using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;

public sealed class RefreshTokenNotFoundError()
    : ApplicationError("Refresh token not found.", StatusCodes.Status404NotFound);
