using FluentValidation;

namespace ECommerce.Application.Features.RefreshToken;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");

}
