using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Application.Features.ForgottenPassword.Events;
using ECommerce.Domain.Entities.PasswordReset;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.ForgottenPassword;
  
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
    public async Task<Result<PasswordResetValidateResponse>> Handle(PasswordResetValidateCommand request, CancellationToken cancellationToken)
    {      
        var passwordResetToken = await GetPasswordResetTokenAsync(request.Token, cancellationToken);
        if (passwordResetToken is null)
        {
            return Result.Fail<PasswordResetValidateResponse>(new InvalidCredentialsError());
        } 

        var user = await GetUserAsync(request.Email, cancellationToken);
        if (user is null || user.OneTimePasswordSecret is null)
        {
            return Result.Fail<PasswordResetValidateResponse>(new InvalidCredentialsError());
        }

        var isValidCode = await IsValidateCodeAsync(user.OneTimePasswordSecret, request.Code);
        if(!isValidCode)
        {
            return Result.Fail<PasswordResetValidateResponse>(new InvalidCredentialsError());
        }
         
        await UpdateRecordsAsync(user, passwordResetToken, request.NewPassword, request.IpAddress, cancellationToken);
        await _publisher.PublishAsync(new PasswordResetCompleted(user.Id, user.FirstName, user.Email, timeProvider.GetUtcNow().UtcDateTime), cancellationToken); 

        return Result.Ok(new PasswordResetValidateResponse("Password successfully changed."));
    }

    private async Task<bool> IsValidateCodeAsync(string otpSecret, string code)
    {
        var decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(otpSecret, encryptionSettings.OneTimePasswordKey);

        return oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code);         
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) => 
                        await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);    

    private async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, CancellationToken cancellationToken)
    {
        var hashedPasswordResetToken = hmacsha256Hasher.HashToken(token, AuthenticationConstants.HashTypeTokenPasswordReset, hashSettings.Secret);

        return await dbContext.PasswordResetTokens
                                .FirstOrDefaultAsync(t => t.TokenHash == hashedPasswordResetToken, cancellationToken);     
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
