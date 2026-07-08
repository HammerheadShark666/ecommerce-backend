using FluentValidation;

namespace ECommerce.Application.Features.Registration.VerifyRegistration;
 
public class VerifyRegistrationCommandValidator : AbstractValidator<VerifyRegistrationCommand>
{
    public VerifyRegistrationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .Must(email => !string.IsNullOrWhiteSpace(email) &&
                   !email.Any(char.IsWhiteSpace))
            .WithMessage("Email is not valid.");
        
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("Code must be 6 characters.");
    }
}
