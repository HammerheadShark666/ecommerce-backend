using ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByPasswordResetToken;
using FluentValidation;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus;
 
public class GetTwoFactorStatusByTokenValidator : AbstractValidator<GetTwoFactorStatusByTokenQuery>
{
    public GetTwoFactorStatusByTokenValidator() => RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidBase64)
            .WithMessage("Invalid password reset token.");

    private static bool BeValidBase64(string token)
    {
        try
        {
            var bytes = Convert.FromBase64String(token);
            return bytes.Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
