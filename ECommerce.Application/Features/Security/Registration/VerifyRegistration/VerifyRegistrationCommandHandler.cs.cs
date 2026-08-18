using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Configuration;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Constants;
using ECommerce.Application.Features.Security.Registration.Events;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.Registration.VerifyRegistration;

internal class VerifyRegistrationCommandHandler(IECommerceDbContext dbContext,
                                                IHmacsha256Hasher hmacsha256Hasher,
                                                IHashSettings hashSettings,
                                                IMessagePublisher _publisher) : ICommandHandler<VerifyRegistrationCommand, VerifyRegistrationResponse>
{
    public async Task<Result<VerifyRegistrationResponse>> Handle(VerifyRegistrationCommand request, CancellationToken cancellationToken)
    {
        var hashedCode = hmacsha256Hasher.HashToken(request.Code, RegistrationConstants.HashTypeVerifyRegistrationEmail, hashSettings.Secret);

        var user = await GetUserAsync(request.Email, hashedCode, cancellationToken);
        if (user is null)
        {
            return Result.Fail<VerifyRegistrationResponse>(new InvalidCredentialsError());
        } 

        await UpdateUser(user, cancellationToken);

        await _publisher.PublishAsync(new UserRegistered(user.Id, user.Email, user.FirstName), cancellationToken);

        return Result.Ok(new VerifyRegistrationResponse());
    }

    private async Task<User?> GetUserAsync(string email, string hashedCode, CancellationToken cancellationToken) => 
                                                    await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email
                                                                        && u.EmailVerificationCode == hashedCode
                                                                        && u.EmailVerificationCodeExpiresAt >= DateTime.UtcNow, cancellationToken);

    private async Task UpdateUser(User user, CancellationToken cancellationToken)
    {
        user.IsEmailVerified = true;
        user.Status = RegistrationConstants.RegistrationActive;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
