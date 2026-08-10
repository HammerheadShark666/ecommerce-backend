using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.CheckTwoFactorStatus;
 
internal class GetTwoFactorStatusQueryHandler(IECommerceDbContext dbContext) : IQueryHandler<GetTwoFactorStatusQuery, GetTwoFactorStatusResponse>
{ 
    public async Task<Result<GetTwoFactorStatusResponse>> Handle(GetTwoFactorStatusQuery request, CancellationToken cancellationToken)
    {
        var normaliseEmail = request.Email.Trim().ToUpperInvariant(); 

        var user = await GetUserAsync(normaliseEmail, cancellationToken);
        if (user is null)
        {
            return Result.Fail<GetTwoFactorStatusResponse>(new UserNotFound());
        }

        return Result.Ok(new GetTwoFactorStatusResponse(user.IsTwoFactorEnabled)); 
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
           
}
