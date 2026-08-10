using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;
 
internal class BeginTwoFactorEnrolmentCommandHandler(IECommerceDbContext dbContext, 
                                                     IOneTimePasswordGenerator oneTimePasswordGenerator, 
                                                     IQrCodeGenerator qrCodeGenerator,
                                                     IAesEncryptionHelper aesEncryptionHelper,
                                                     IJwtSettings jwtSettings,
                                                     IEncryptionSettings encryptionSettings) : ICommandHandler<BeginTwoFactorEnrolmentCommand, BeginTwoFactorEnrolmentResponse>
{
    public async Task<Result<BeginTwoFactorEnrolmentResponse>> Handle(BeginTwoFactorEnrolmentCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Fail<BeginTwoFactorEnrolmentResponse>(new InvalidCredentialsError());
        }

        if (user.IsTwoFactorEnabled)
        {
            return Result.Fail<BeginTwoFactorEnrolmentResponse>(new TwofaAlreadyEnabledError()); 
        }

        (var oneTimePasswordSecret, var encryptedOneTimePasswordSecret) = await GenerateAndEncryptOneTimePasswordSecretAsync();
        await UpdateUser(user, encryptedOneTimePasswordSecret, cancellationToken);
        (var qrBase64, var uri) = GenerateQrCode(request.Email, oneTimePasswordSecret);

        return Result.Ok(new BeginTwoFactorEnrolmentResponse(qrBase64, uri));
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            

    private async Task<(string secret, string encryptedSecrete)> GenerateAndEncryptOneTimePasswordSecretAsync()
    {
        var secret = oneTimePasswordGenerator.GenerateSecret();
        var encryptedSecrect = aesEncryptionHelper.Encrypt(secret, encryptionSettings.OneTimePasswordKey);

        return (secret, encryptedSecrect);
    }

    private async Task UpdateUser(User user, string encryptedOneTimePasswordSecret, CancellationToken cancellationToken)
    {
        user.OneTimePasswordSecret = encryptedOneTimePasswordSecret;
        user.IsTwoFactorEnabled = false;       
         
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private (string qrBase64, string uri) GenerateQrCode(string email, string secret)
    {
        var uri = qrCodeGenerator.BuildOneTimePasswordAuthUri(jwtSettings.Issuer, email, secret);
        var qrBase64 = qrCodeGenerator.GenerateQrCodeBase64(uri);

        return (qrBase64, uri);
    }
}
