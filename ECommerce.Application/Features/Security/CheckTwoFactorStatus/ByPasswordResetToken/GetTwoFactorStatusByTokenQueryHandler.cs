using ECommerce.Application.Abstractions;
using ECommerce.Application.Abstractions.Messaging;
using ECommerce.Application.Common.Errors;
using ECommerce.Domain.Entities.User;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Features.Security.CheckTwoFactorStatus.ByPasswordResetToken;
 
internal class GetTwoFactorStatusByTokenQueryHandler(IECommerceDbContext dbContext) : IQueryHandler<GetTwoFactorStatusByTokenQuery, GetTwoFactorStatusByTokenResponse>
{ 
    public async Task<Result<GetTwoFactorStatusByTokenResponse>> Handle(GetTwoFactorStatusByTokenQuery request, CancellationToken cancellationToken)
    {
        var token = request.Token.Trim().ToUpperInvariant(); 

        var user = await GetUserAsync(token, cancellationToken);
        if (user is null)
        {
            return Result.Fail<GetTwoFactorStatusByTokenResponse>(new InvalidCredentialsError());
        }

        return Result.Ok(new GetTwoFactorStatusByTokenResponse(user.IsTwoFactorEnabled)); 
    }

    private async Task<User?> GetUserAsync(string token, CancellationToken cancellationToken) =>
                await dbContext.PasswordResetTokens
                    .Where(x => x.TokenHash == token)
                    .Select(x => x.User)
                    .FirstOrDefaultAsync(cancellationToken);           
}
