using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.RequestRegistrationVerifyEmail;

internal class RequestVerifyRegistrationEmailCommandHandler(IECommerceDbContext dbContext,
                                               IMessagePublisher _publisher) : ICommandHandler<RequestVerifyRegistrationEmailCommand, RequestVerifyRegistrationEmailResponse>
{
    public async Task<Result<RequestVerifyRegistrationEmailResponse>> Handle(RequestVerifyRegistrationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Fail<RequestVerifyRegistrationEmailResponse>(new InvalidCredentialsError());
        }

        if (user.IsEmailVerified == false)
        {
            await _publisher.PublishAsync(new VerifyRegistrationEmail(user.Id, user.Email, user.FirstName), cancellationToken);
        }
        else
        {
            return Result.Ok(new RequestVerifyRegistrationEmailResponse("Registration has already been verified."));
        }
        
        return Result.Ok(new RequestVerifyRegistrationEmailResponse());
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                                await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
