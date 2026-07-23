using FluentValidation;

namespace ECommerce.Application.Features.Registration.RequestRegistrationVerifyEmail;

public class RequestVerifyRegistrationEmailCommandValidator : AbstractValidator<RequestVerifyRegistrationEmailCommand>
{
    public RequestVerifyRegistrationEmailCommandValidator() => RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .Must(email => !string.IsNullOrWhiteSpace(email) &&
                   !email.Any(char.IsWhiteSpace))
            .WithMessage("Email is not valid.");
}
