using System.Net;
using FluentValidation;

namespace ECommerce.Application.Features.ForgottenPassword;

public class PasswordResetValidateCommandValidator : AbstractValidator<PasswordResetValidateCommand>
{
    public PasswordResetValidateCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .Must(x =>
            {
                try
                {
                    return Convert.FromBase64String(x).Length == 32;
                }
                catch
                {
                    return false;
                }
            });

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MinimumLength(6).WithMessage("Code must be 6 characters.");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .Must(ip => IPAddress.TryParse(ip, out _))
            .WithMessage("'{PropertyValue}' is not a valid IP address."); 
    }
}
