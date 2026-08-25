using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByEmail;

public record GetTwoFactorStatusByEmailQuery(string Email) : IQuery<GetTwoFactorStatusByEmailQueryResponse>;

public record GetTwoFactorStatusByEmailQueryResponse(bool IsEnabled);
