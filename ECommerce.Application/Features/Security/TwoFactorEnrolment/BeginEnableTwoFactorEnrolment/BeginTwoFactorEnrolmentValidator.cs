using FluentValidation;

namespace ECommerce.Application.Features.Security.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;
 

public class BeginTwoFactorEnrolmentValidator : AbstractValidator<BeginTwoFactorEnrolmentCommand>
{
    public BeginTwoFactorEnrolmentValidator() => RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");
}
