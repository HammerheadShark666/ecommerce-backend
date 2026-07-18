using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Constants;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.ForgottenPassword.Events;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.ForgottenPassword;
 
public record PasswordResetValidateCommand(string Token, string Email, string NewPassword, string Code, string IpAddress) : ICommand<PasswordResetValidateResponse>;

public record PasswordResetValidateResponse(string Message);

internal class PasswordResetValidateCommandHandler(IECommerceDbContext dbContext,
                                                   IAesEncryptionHelper aesEncryptionHelper,
                                                   IEncryptionSettings encryptionSettings,   
                                                   IPasswordHasher passwordHasher,
                                                   IHmacsha256Hasher hmacsha256Hasher,
                                                   IHashSettings hashSettings,
                                                   TimeProvider timeProvider,
                                                   IOneTimePasswordGenerator oneTimePasswordGenerator,
                                                   IMessagePublisher _publisher) : ICommandHandler<PasswordResetValidateCommand, PasswordResetValidateResponse>
{
    private static readonly TimeSpan PendingTokenTtl = TimeSpan.FromMinutes(5);

    public async Task<PasswordResetValidateResponse> Handle(PasswordResetValidateCommand request, CancellationToken cancellationToken)
    {
        //Validate token
        PasswordResetToken passwordResetToken = await GetPasswordResetTokenAsync(request.Token, cancellationToken);

        //Validate code
        (User user, string otpSecret) = await GetUserAndSecretAsync(request.Email, cancellationToken);
        bool codeIsValid = await ValidateCodeAsync(otpSecret, request.Code);

        if (!codeIsValid)
        {
            throw new UnauthorizedAccessException();
        }

        await UpdateRecordsAsync(user, passwordResetToken, request.NewPassword, request.IpAddress, cancellationToken);
        await _publisher.PublishAsync(new PasswordResetCompleted(user.Id, user.FirstName, user.Email, timeProvider.GetUtcNow().UtcDateTime), cancellationToken); 

        return new PasswordResetValidateResponse("Password successfully changed.");
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

    private async Task<(User user, string otpSecret)> GetUserAndSecretAsync(string email, CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException(nameof(User), email); 

        if (user.OneTimePasswordSecret is null)
        {
            throw new TwoFactorEnrolmentNotStartedException();
        }  

        return (user, user.OneTimePasswordSecret);
    }

    private async Task<PasswordResetToken> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
    { 
        string hashedPasswordResetToken = hmacsha256Hasher.HashToken(token, AuthenticationConstants.HashTypeTokenPasswordReset, hashSettings.Secret);

        return await dbContext.PasswordResetTokens
                                .FirstOrDefaultAsync(t => t.TokenHash == hashedPasswordResetToken, cancellationToken) 
                                ?? throw new UnauthorizedAccessException();
    }

    private async Task UpdateRecordsAsync(User user, PasswordResetToken passwordResetToken, string newPassword, string ipAddress, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHasher.Hash(newPassword);
        passwordResetToken.Used = true;
        passwordResetToken.UsedAt = timeProvider.GetUtcNow().UtcDateTime;
        passwordResetToken.CreatedByIp = ipAddress;

        await dbContext.SaveChangesAsync(cancellationToken);
    } 
}
