using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Application.Features.Security.Registration.Events;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.Registration.BeginRegistration;

internal class BeginRegistrationCommandHandler(IECommerceDbContext dbContext,
                                               IOneTimePasswordGenerator oneTimePasswordGenerator,
                                               IMessagePublisher _publisher,
                                               IPasswordHasher passwordHasher,                                        
                                               IAesEncryptionHelper aesEncryptionHelper,                                                
                                               IEncryptionSettings encryptionSettings) : ICommandHandler<BeginRegistrationCommand, BeginRegistrationResponse>                                        
{    
    public async Task<Result<BeginRegistrationResponse>> Handle(BeginRegistrationCommand request, CancellationToken cancellationToken)
    { 
        var emailExists = await EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Fail<BeginRegistrationResponse>(new ConflictError("An account with this email already exists."));
        }

        (var oneTimePasswordSecret, var encryptedOneTimePasswordSecret) = await GenerateAndEncryptOneTimePasswordSecretAsync();

        var user = await CreateUserAsync(request.Email, request.Password, encryptedOneTimePasswordSecret,
                             request.LastName, request.FirstName, request.PhoneNumber, cancellationToken);

        await _publisher.PublishAsync(new VerifyRegistrationEmail(user.Id, user.Email, user.FirstName), cancellationToken);         

        return Result.Ok(new BeginRegistrationResponse());
    }

    private async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => 
                        await dbContext.Users
                            .AsNoTracking()
                            .AnyAsync(u => u.Email == email, cancellationToken); 

    private async Task<(string secret, string encryptedSecrete)> GenerateAndEncryptOneTimePasswordSecretAsync()
    {
        var secret = oneTimePasswordGenerator.GenerateSecret();
        var encryptedSecrect = aesEncryptionHelper.Encrypt(secret, encryptionSettings.OneTimePasswordKey);

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
            Status = RegistrationConstants.RegistrationInActive
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return user;
    } 
}
