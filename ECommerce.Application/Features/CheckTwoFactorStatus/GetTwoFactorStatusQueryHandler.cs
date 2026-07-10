using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.CheckTwoFactorStatus;

public record GetTwoFactorStatusQuery(string Email) : IQuery<GetTwoFactorStatusResponse>;

public record GetTwoFactorStatusResponse(bool IsEnabled);

internal class GetTwoFactorStatusQueryHandler(IECommerceDbContext dbContext) : IQueryHandler<GetTwoFactorStatusQuery, GetTwoFactorStatusResponse>
{ 
    public async Task<GetTwoFactorStatusResponse> Handle(GetTwoFactorStatusQuery request, CancellationToken cancellationToken)
    {
        User user = await GetUserAsync(request.Email, cancellationToken);
        return new GetTwoFactorStatusResponse(user.IsTwoFactorEnabled);
    }

    private async Task<User> GetUserAsync(string email, CancellationToken cancellationToken) => await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException(nameof(User), email);
}
