using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.CheckTwoFactorStatus;

public record GetTwoFactorStatusQuery(string Email) : IQuery<GetTwoFactorStatusResponse>;

public record GetTwoFactorStatusResponse(bool IsEnabled);
