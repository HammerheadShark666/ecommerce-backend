using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.BeginRegistration;

public record BeginRegistrationCommand(string Email, string Password, string ConfirmPassword, string LastName, string FirstName, string PhoneNumber) : ICommand<BeginRegistrationResponse>;

public record BeginRegistrationResponse(
    string QrCodeBase64,   // embed: <img src="data:image/png;base64,{value}" />
    string OtpAuthUri      // otpauth:// URI (useful for deep-linking on mobile)
);

internal class BeginRegistrationCommandHandler(IECommerceDbContext dbContext,
                                               IOneTimePasswordGenerator oneTimePasswordGenerator,
                                               IQrCodeGenerator qrCodeGenerator,
                                               IPasswordHasher passwordHasher,                                        
                                               IAesEncryptionHelper aesEncryptionHelper,
                                               IJwtSettings jwtSettings,
                                               IEncryptionSettings encryptionSettings) : ICommandHandler<BeginRegistrationCommand, BeginRegistrationResponse>                                        
{    
    public async Task<BeginRegistrationResponse> Handle(BeginRegistrationCommand request, CancellationToken cancellationToken)
    { 
        await ValidateRegistrationDetails(request.Email, cancellationToken);

        (string oneTimePasswordSecret, string encryptedOneTimePasswordSecret) = await GenerateAndEncryptOneTimePasswordSecretAsync();
         
        await CreateUserAsync(request.Email, request.Password, encryptedOneTimePasswordSecret, 
                            request.LastName, request.FirstName, request.PhoneNumber, cancellationToken); 
         
        (string qrBase64, string uri) = GenerateQrCode(request.Email, oneTimePasswordSecret);

        return new BeginRegistrationResponse(qrBase64, uri);
    }

    private async Task ValidateRegistrationDetails(string email, CancellationToken cancellationToken)
    {
        if (await dbContext.Users
                            .AsNoTracking()
                            .AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new DuplicateEmailException($"User with email '{email}' already exists.");
        }
    }

    private async Task<(string secret, string encryptedSecrete)> GenerateAndEncryptOneTimePasswordSecretAsync()
    {
        string secret = oneTimePasswordGenerator.GenerateSecret();     
        string encryptedSecrect = aesEncryptionHelper.Encrypt(secret, encryptionSettings.OneTimePasswordKey);

        return (secret, encryptedSecrect);
    }

    private async Task CreateUserAsync(string email, string password, string encryptedOneTimePasswordSecret, string lastName, string firstName, string phoneNumber, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Email = email.Trim().ToUpperInvariant(),
            LastName = lastName,
            FirstName = firstName,
            Phone = phoneNumber,
            PasswordHash = passwordHasher.Hash(password),
            OneTimePasswordSecret = encryptedOneTimePasswordSecret,
            IsTwoFactorEnabled = false,
            Status = "Active"            
        }; 

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private (string qrBase64, string uri) GenerateQrCode(string email, string secret)
    {
        string uri = qrCodeGenerator.BuildOneTimePasswordAuthUri(jwtSettings.Issuer, email, secret);
        string qrBase64 = qrCodeGenerator.GenerateQrCodeBase64(uri);

        return (qrBase64, uri);
    }
}
