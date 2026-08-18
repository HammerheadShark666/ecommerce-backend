using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;
 
internal class ConfirmTwoFactorEnrolmentCommandHandler(IECommerceDbContext dbContext,
                                                       IAesEncryptionHelper aesEncryptionHelper,                                                      
                                                       IEncryptionSettings encryptionSettings,
                                                       IOneTimePasswordGenerator oneTimePasswordGenerator) : ICommandHandler<ConfirmTwoFactorEnrolmentCommand, ConfirmTwoFactorEnrolmentResponse>
{ 
    public async Task<Result<ConfirmTwoFactorEnrolmentResponse>> Handle(ConfirmTwoFactorEnrolmentCommand request, CancellationToken cancellationToken)
    { 
        var user = await GetUserAsync(request.Email, cancellationToken);
        if (user is null || user.OneTimePasswordSecret is null)
        {
            return Result.Fail<ConfirmTwoFactorEnrolmentResponse>(new InvalidCredentialsError());
        }

        if (user.IsTwoFactorEnabled)
        {
            return Result.Fail<ConfirmTwoFactorEnrolmentResponse>(new TwofaAlreadyEnabledError());
        }

        var codeIsValid = await ValidateCodeAsync(user.OneTimePasswordSecret, request.Code);
        if (!codeIsValid)
        {
            return Result.Fail<ConfirmTwoFactorEnrolmentResponse>(new InvalidCredentialsError());
        }

        await UpdateTwoFactorEnabledState(user, cancellationToken);

        return Result.Ok(new ConfirmTwoFactorEnrolmentResponse("2FA enabled successfully."));             
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) => 
                        await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken); 

    private async Task<bool> ValidateCodeAsync(string otpSecret, string code)
    {
        var decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(otpSecret, encryptionSettings.OneTimePasswordKey);
        return  oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code);      
    }

    private async Task UpdateTwoFactorEnabledState(User user, CancellationToken cancellationToken)
    {
        user.IsTwoFactorEnabled = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
