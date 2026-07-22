using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Constants;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.VerifyRegistration;

public record VerifyRegistrationCommand(string Email, string Code) : ICommand<VerifyRegistrationResponse>;

public record VerifyRegistrationResponse(bool Success, string Message);

internal class VerifyRegistrationCommandHandler(IECommerceDbContext dbContext,
                                                IHmacsha256Hasher hmacsha256Hasher,
                                                IHashSettings hashSettings,
                                                IMessagePublisher _publisher) : ICommandHandler<VerifyRegistrationCommand, VerifyRegistrationResponse>
{
    public async Task<VerifyRegistrationResponse> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        string hashedCode = hmacsha256Hasher.HashToken(request.Code, RegistrationConstants.HashTypeVerifyRegistrationEmail, hashSettings.Secret);

        User user = await GetUser(request.Email, hashedCode, cancellationToken);         
        await UpdateUser(user, cancellationToken); 

        await _publisher.PublishAsync(new UserRegistered(user.Id, user.Email, user.FirstName), cancellationToken);

        return new VerifyRegistrationResponse(true, "Registration verified"); 
    }

    private async Task<User> GetUser(string email, string hashedCode, CancellationToken cancellationToken) => 
                                await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email 
                                                                          && u.EmailVerificationCode == hashedCode
                                                                          && u.EmailVerificationCodeExpiresAt >= DateTime.UtcNow, cancellationToken)
                                     ?? throw new VerificationCodeExpiredException();


    private async Task UpdateUser(User user, CancellationToken cancellationToken)
    {
        user.IsEmailVerified = true;
        user.Status = RegistrationConstants.RegistrationActive;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
