using FluentValidation;

namespace ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

public class ConfirmTwoFactorEnrolmentValidator : AbstractValidator<ConfirmTwoFactorEnrolmentCommand>
{
    public ConfirmTwoFactorEnrolmentValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("Code must be 6 characters.");
    }
}
