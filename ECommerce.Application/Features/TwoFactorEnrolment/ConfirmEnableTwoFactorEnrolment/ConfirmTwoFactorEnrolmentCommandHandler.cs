using ECommerce.Core.Abstractions;
using ECommerce.Core.Abstractions.Configuration;
using ECommerce.Core.Abstractions.Messaging;
using ECommerce.Core.Exceptions;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

public record ConfirmTwoFactorEnrolmentCommand(string Email, string Code) : ICommand<ConfirmTwoFactorEnrolmentResponse>;

public record ConfirmTwoFactorEnrolmentResponse(bool Success, string Message);

internal class ConfirmTwoFactorEnrolmentCommandHandler(IECommerceDbContext dbContext,
                                                       IAesEncryptionHelper aesEncryptionHelper,                                                      
                                                       IEncryptionSettings encryptionSettings,
                                                       IOneTimePasswordGenerator oneTimePasswordGenerator) : ICommandHandler<ConfirmTwoFactorEnrolmentCommand, ConfirmTwoFactorEnrolmentResponse>
{ 
    public async Task<ConfirmTwoFactorEnrolmentResponse> Handle(ConfirmTwoFactorEnrolmentCommand request, CancellationToken cancellationToken)
    {
        (User? user, string? otpSecret) = await GetUserAndSecretAsync(request.Email, cancellationToken);

        bool codeIsValid = await ValidateCodeAsync(otpSecret, request.Code);
        await UpdateTwoFactorEnabledState(user, cancellationToken);



        return codeIsValid
            ? new ConfirmTwoFactorEnrolmentResponse(true, "2FA enabled successfully.")
            : new ConfirmTwoFactorEnrolmentResponse(false, "Invalid or expired code. Please try again.");
    }

    private async Task<(User user, string otpSecret)> GetUserAndSecretAsync(string email, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException(nameof(User), email);
        ;

        if (user.OneTimePasswordSecret is null)
        {
            throw new TwoFactorEnrolmentNotStartedException();
        }

        if (user.IsTwoFactorEnabled)
        {
            throw new InvalidTwoFactorStateException("2FA is already confirmed and enabled.");
        }

        if (user.OneTimePasswordSecret is null)
        {
            throw new TwoFactorEnrolmentNotStartedException();
        }

        return (user, user.OneTimePasswordSecret);
    }

    private async Task<bool> ValidateCodeAsync(string otpSecret, string code)
    {
        string decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(otpSecret, encryptionSettings.OneTimePasswordKey);

        bool valid = oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code);
        if (!valid)
        {
            throw new UnauthorizedAccessException();
        }

        return true;
    }

    private async Task UpdateTwoFactorEnabledState(User user, CancellationToken cancellationToken)
    {
        user.IsTwoFactorEnabled = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
