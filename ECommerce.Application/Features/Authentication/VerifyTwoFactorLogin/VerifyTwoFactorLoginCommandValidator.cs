using FluentValidation;

namespace ECommerce.Application.Features.Authentication.VerifyTwoFactorLogin;

 
public class VerifyTwoFactorLoginCommandValidator : AbstractValidator<VerifyTwoFactorLoginCommand>
{
    public VerifyTwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.PendingToken)
            .NotEmpty().WithMessage("Pending token is required.")
            .MinimumLength(8).WithMessage("Pending token must be at least 8 characters.")
            .MaximumLength(45).WithMessage("Pending token must be less than 35 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("Code must be 6 characters.");
    }
}
