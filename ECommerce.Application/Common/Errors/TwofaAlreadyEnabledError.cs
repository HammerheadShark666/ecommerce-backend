using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Errors;

public sealed class TwofaAlreadyEnabledError()
    : ApplicationError("2fa already enabled for this user.", StatusCodes.Status401Unauthorized);
