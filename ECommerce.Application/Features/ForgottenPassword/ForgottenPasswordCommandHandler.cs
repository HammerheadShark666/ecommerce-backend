using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Features.ForgottenPassword.Events;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.ForgottenPassword;

public record ForgottenPasswordCommand(string Email) : ICommand<ForgottenPasswordResponse>;

public record ForgottenPasswordResponse(string Message);


internal class ForgottenPasswordCommandHandler(IECommerceDbContext dbContext,
                                               IMessagePublisher _publisher) : ICommandHandler<ForgottenPasswordCommand, ForgottenPasswordResponse>
{
    private static readonly TimeSpan PendingTokenTtl = TimeSpan.FromMinutes(5);

    public async Task<ForgottenPasswordResponse> Handle(ForgottenPasswordCommand request, CancellationToken cancellationToken)
    { 
        string normaliseEmail = request.Email.Trim().ToUpperInvariant();
        User user = await GetUserAsync(normaliseEmail, cancellationToken);

        if(user is not null)
        {
            await _publisher.PublishAsync(new ResetPasswordRequested(user.Id, user.Email), cancellationToken); 
        }
 

        return new ForgottenPasswordResponse("If an account exists for that email, a reset link has been sent.");
    } 
    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken) =>
          await dbContext.Users
                   .AsNoTracking()
                   .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
          ?? throw new UnauthorizedAccessException();
}
