using FluentValidation;

namespace ECommerce.Application.Features.Registration.BeginRegistration;

public class BeginRegistrationCommandValidator : AbstractValidator<BeginRegistrationCommand>
{
    public BeginRegistrationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .Must(email => !string.IsNullOrWhiteSpace(email) &&
                   !email.Any(char.IsWhiteSpace))
            .WithMessage("Email is not valid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .MinimumLength(8).WithMessage("Confirm password must be at least 8 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Must((model, confirmPassword) => confirmPassword == model.Password)
            .WithMessage("Passwords do not match.");
    }
}
