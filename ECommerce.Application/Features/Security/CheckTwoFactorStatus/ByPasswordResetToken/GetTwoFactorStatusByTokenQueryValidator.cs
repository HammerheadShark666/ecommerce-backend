using System.Buffers.Text;
using FluentValidation;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByPasswordResetToken;
 
public class GetTwoFactorStatusByTokenQueryValidator : AbstractValidator<GetTwoFactorStatusByTokenQuery>
{
    public GetTwoFactorStatusByTokenQueryValidator() => RuleFor(x => x.Token)
            .NotEmpty()
            .MaximumLength(100)
            .Must(BeValidBase64Url)
            .WithMessage("Invalid password reset token.");

    private static bool BeValidBase64Url(string token)
    {
        Span<byte> buffer = stackalloc byte[32];
        return Base64Url.TryDecodeFromChars(token, buffer, out var bytesWritten)
            && bytesWritten == 32;
    }
}
