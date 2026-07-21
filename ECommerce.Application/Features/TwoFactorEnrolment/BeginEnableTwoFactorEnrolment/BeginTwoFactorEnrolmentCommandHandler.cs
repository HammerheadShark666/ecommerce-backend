using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.TwoFactorEnrolment.BeginEnableTwoFactorEnrolment;

public record BeginTwoFactorEnrolmentCommand(string Email) : ICommand<BeginTwoFactorEnrolmentResponse>;

public record BeginTwoFactorEnrolmentResponse(
    string QrCodeBase64,   // embed: <img src="data:image/png;base64,{value}" />
    string OtpAuthUri      // otpauth:// URI (useful for deep-linking on mobile)
);

internal class BeginTwoFactorEnrolmentCommandHandler(IECommerceDbContext dbContext, 
                                                     IOneTimePasswordGenerator oneTimePasswordGenerator, 
                                                     IQrCodeGenerator qrCodeGenerator,
                                                     IAesEncryptionHelper aesEncryptionHelper,
                                                     IJwtSettings jwtSettings,
                                                     IEncryptionSettings encryptionSettings) : ICommandHandler<BeginTwoFactorEnrolmentCommand, BeginTwoFactorEnrolmentResponse>
{
    public async Task<BeginTwoFactorEnrolmentResponse> Handle(BeginTwoFactorEnrolmentCommand request, CancellationToken cancellationToken)
    {
        User user = await GetUserAsync(request.Email, cancellationToken);

        if (user.IsTwoFactorEnabled)
        {
            throw new InvalidTwoFactorStateException("2FA is already enabled for this user.");
        }

        (string oneTimePasswordSecret, string encryptedOneTimePasswordSecret) = await GenerateAndEncryptOneTimePasswordSecretAsync();
        await UpdateUser(user, encryptedOneTimePasswordSecret, cancellationToken);
        (string qrBase64, string uri) = GenerateQrCode(request.Email, oneTimePasswordSecret);

        return new BeginTwoFactorEnrolmentResponse(qrBase64, uri); 
    }

    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken) => await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException(nameof(User), email);

    private async Task<(string secret, string encryptedSecret)> GenerateAndEncryptOneTimePasswordSecretAsync()
    {
        string secret = oneTimePasswordGenerator.GenerateSecret();
        string encryptedSecret = aesEncryptionHelper.Encrypt(secret, encryptionSettings.OneTimePasswordKey);

        return (secret, encryptedSecret);
    }

    private async Task UpdateUser(User user, string encryptedOneTimePasswordSecret, CancellationToken cancellationToken)
    {
        user.OneTimePasswordSecret = encryptedOneTimePasswordSecret;
        user.IsTwoFactorEnabled = false;       
         
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private (string qrBase64, string uri) GenerateQrCode(string email, string secret)
    {
        string uri = qrCodeGenerator.BuildOneTimePasswordAuthUri(jwtSettings.Issuer, email, secret);
        string qrBase64 = qrCodeGenerator.GenerateQrCodeBase64(uri);

        return (qrBase64, uri);
    }
}
