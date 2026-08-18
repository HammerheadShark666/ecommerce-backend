using FluentValidation;

namespace ECommerce.Application.Features.Security.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");

}
