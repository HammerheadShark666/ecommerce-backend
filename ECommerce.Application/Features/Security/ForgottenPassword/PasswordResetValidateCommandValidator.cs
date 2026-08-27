using System.Buffers.Text;
using System.Net;
using FluentValidation;

namespace ECommerce.Application.Features.Security.ForgottenPassword;

public class PasswordResetValidateCommandValidator : AbstractValidator<PasswordResetValidateCommand>
{
    public PasswordResetValidateCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidBase64Url)
            .WithMessage("Invalid password reset token.");    

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .Must(ip => IPAddress.TryParse(ip, out _))
            .WithMessage("'{PropertyValue}' is not a valid IP address."); 
    }

    private static bool BeValidBase64Url(string token)
    {
        Span<byte> buffer = stackalloc byte[32];
        return Base64Url.TryDecodeFromChars(token, buffer, out var bytesWritten)
            && bytesWritten == 32;
    }
}
