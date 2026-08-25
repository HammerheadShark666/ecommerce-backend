using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByEmail;
 
internal class GetTwoFactorStatusByEmailQueryHandler(IECommerceDbContext dbContext) : IQueryHandler<GetTwoFactorStatusByEmailQuery, GetTwoFactorStatusByEmailQueryResponse>
{ 
    public async Task<Result<GetTwoFactorStatusByEmailQueryResponse>> Handle(GetTwoFactorStatusByEmailQuery request, CancellationToken cancellationToken)
    {
        var normaliseEmail = request.Email.Trim().ToUpperInvariant(); 

        var user = await GetUserAsync(normaliseEmail, cancellationToken);
        if (user is null)
        {
            return Result.Fail<GetTwoFactorStatusByEmailQueryResponse>(new InvalidCredentialsError());
        }

        return Result.Ok(new GetTwoFactorStatusByEmailQueryResponse(user.IsTwoFactorEnabled)); 
    }

    private async Task<User?> GetUserAsync(string email, CancellationToken cancellationToken) =>
                        await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
           
}
