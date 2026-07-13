using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.VerifyRegistration;

public record VerifyRegistrationCommand(string Email, string Code) : ICommand<VerifyRegistrationResponse>;

public record VerifyRegistrationResponse(bool Success, string Message);

internal class VerifyRegistrationCommandHandler(IECommerceDbContext dbContext,
                                                IAesEncryptionHelper aesEncryptionHelper,                                              
                                                IEncryptionSettings encryptionSettings,
                                                IMessagePublisher _publisher,
                                                IOneTimePasswordGenerator oneTimePasswordGenerator) : ICommandHandler<VerifyRegistrationCommand, VerifyRegistrationResponse>
{
    public async Task<VerifyRegistrationResponse> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        (User? user, string? otpSecret) = await GetUserAndSecretAsync(request.Email, cancellationToken);
               
        bool codeIsValid = await ValidateCodeAsync(otpSecret, request.Code);
        await UpdateTwoFactorEnabledState(user, cancellationToken);

        if (codeIsValid)
        {
            await _publisher.PublishAsync(new UserRegistered(user.Id, user.Email, user.FirstName), cancellationToken);
            return new VerifyRegistrationResponse(true, "Registration verified");
        }
        else
        {
            return new VerifyRegistrationResponse(false, "Invalid or expired code. Please try again.");
        }
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
