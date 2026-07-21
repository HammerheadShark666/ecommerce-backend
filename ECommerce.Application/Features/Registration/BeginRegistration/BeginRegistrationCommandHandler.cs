using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.BeginRegistration;

public record BeginRegistrationCommand(string Email, string Password, string ConfirmPassword, string LastName, string FirstName, string PhoneNumber) : ICommand<BeginRegistrationResponse>;

public record BeginRegistrationResponse(
    string Message = "Registration initiated successfully.  Email sent to verify email."
);

internal class BeginRegistrationCommandHandler(IECommerceDbContext dbContext,
                                               IOneTimePasswordGenerator oneTimePasswordGenerator,
                                               IMessagePublisher _publisher,
                                               IPasswordHasher passwordHasher,                                        
                                               IAesEncryptionHelper aesEncryptionHelper,
                                               IEncryptionSettings encryptionSettings) : ICommandHandler<BeginRegistrationCommand, BeginRegistrationResponse>                                        
{    
    public async Task<BeginRegistrationResponse> Handle(BeginRegistrationCommand request, CancellationToken cancellationToken)
    { 
        await ValidateRegistrationDetails(request.Email, cancellationToken);

        (string oneTimePasswordSecret, string encryptedOneTimePasswordSecret) = await GenerateAndEncryptOneTimePasswordSecretAsync();
         
        User user = await CreateUserAsync(request.Email, request.Password, encryptedOneTimePasswordSecret, 
                            request.LastName, request.FirstName, request.PhoneNumber, cancellationToken);

        await _publisher.PublishAsync(new VerifyRegistrationEmail(user.Id, user.Email, user.FirstName), cancellationToken);

        return new BeginRegistrationResponse();
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

    private async Task<User> CreateUserAsync(string email, string password, string encryptedOneTimePasswordSecret, string lastName, string firstName, string phoneNumber, CancellationToken cancellationToken)
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

        return user;
    }
}
