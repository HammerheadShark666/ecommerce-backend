using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.TwoFactorEnrolment.ConfirmEnableTwoFactorEnrolment;

public record ConfirmTwoFactorEnrolmentCommand(string Email, string Code) : ICommand<ConfirmTwoFactorEnrolmentResponse>;

public record ConfirmTwoFactorEnrolmentResponse(string Message);

internal class ConfirmTwoFactorEnrolmentCommandHandler(IECommerceDbContext dbContext,
                                                       IAesEncryptionHelper aesEncryptionHelper,                                                      
                                                       IEncryptionSettings encryptionSettings,
                                                       IOneTimePasswordGenerator oneTimePasswordGenerator) : ICommandHandler<ConfirmTwoFactorEnrolmentCommand, ConfirmTwoFactorEnrolmentResponse>
{ 
    public async Task<Result<ConfirmTwoFactorEnrolmentResponse>> Handle(ConfirmTwoFactorEnrolmentCommand request, CancellationToken cancellationToken)
    {
        (var user, var otpSecret) = await GetUserAndSecretAsync(request.Email, cancellationToken);

        var codeIsValid = await ValidateCodeAsync(otpSecret, request.Code);
        await UpdateTwoFactorEnabledState(user, cancellationToken);

        return codeIsValid
            ? Result.Ok(new ConfirmTwoFactorEnrolmentResponse("2FA enabled successfully."))
            : Result.Fail(new InvalidCredentialsError());
    }

    private async Task<(User user, string otpSecret)> GetUserAndSecretAsync(string email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException(nameof(User), email);
         
        if (user.OneTimePasswordSecret is null)
        {
            throw new TwoFactorEnrolmentNotStartedException();
        }

        if (user.IsTwoFactorEnabled)
        {
            throw new InvalidTwoFactorStateException("2FA is already confirmed and enabled.");
        } 

        return (user, user.OneTimePasswordSecret); 
    }

    private async Task<bool> ValidateCodeAsync(string otpSecret, string code)
    {
        var decryptedOneTimePasswordSecret = aesEncryptionHelper.Decrypt(otpSecret, encryptionSettings.OneTimePasswordKey);

        var valid = oneTimePasswordGenerator.VerifyCode(decryptedOneTimePasswordSecret, code);
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
