using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByPasswordResetToken;

public record GetTwoFactorStatusByTokenQuery(string Token) : IQuery<GetTwoFactorStatusByTokenResponse>;

public record GetTwoFactorStatusByTokenResponse(bool IsEnabled);
