using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;

public sealed class InvalidCredentialsError()
    : ApplicationError("The provided credentials are invalid.", StatusCodes.Status401Unauthorized);
