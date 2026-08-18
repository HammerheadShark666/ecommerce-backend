using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Features.Security.ForgottenPassword.Events;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.ForgottenPassword;

internal class ForgottenPasswordCommandHandler(IECommerceDbContext dbContext,
                                               IMessagePublisher _publisher) : ICommandHandler<ForgottenPasswordCommand, ForgottenPasswordResponse>
{
    public async Task<Result<ForgottenPasswordResponse>> Handle(ForgottenPasswordCommand request, CancellationToken cancellationToken)
    {
        var normaliseEmail = request.Email.Trim().ToUpperInvariant();
        var user = await GetUserAsync(normaliseEmail, cancellationToken);
        if (user is null)
        {
            return Result.Fail<ForgottenPasswordResponse>(new InvalidCredentialsError());
        }
      
        await _publisher.PublishAsync(new PasswordResetRequested(user.Id, user.FirstName, user.Email), cancellationToken); 
      
        return Result.Ok(new ForgottenPasswordResponse("If an account exists for that email, a reset link has been sent."));
    } 
   
    private Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        dbContext.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
