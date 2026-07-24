using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Registration.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Registration.RequestRegistrationVerifyEmail;

public record RequestVerifyRegistrationEmailCommand(string Email) : ICommand<RequestVerifyRegistrationEmailResponse>;

public record RequestVerifyRegistrationEmailResponse(
    string Message = "Send verify registration email initiated successfully."
);

internal class RequestVerifyRegistrationEmailCommandHandler(IECommerceDbContext dbContext,
                                               IMessagePublisher _publisher) : ICommandHandler<RequestVerifyRegistrationEmailCommand, RequestVerifyRegistrationEmailResponse>
{
    public async Task<RequestVerifyRegistrationEmailResponse> Handle(RequestVerifyRegistrationEmailCommand request, CancellationToken cancellationToken)
    {
        User user = await GetUser(request.Email, cancellationToken);

        await _publisher.PublishAsync(new VerifyRegistrationEmail(user.Id, user.Email, user.FirstName), cancellationToken);

        return new RequestVerifyRegistrationEmailResponse();
    }

    private async Task<User> GetUser(string email, CancellationToken cancellationToken) =>
                                await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email
                                                                          && u.IsEmailVerified == false, cancellationToken)
                                     ?? throw new RegistrationEmailAlreadyVerifiedException();
}
