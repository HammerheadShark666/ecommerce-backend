using System.Security.Claims;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Library.Authentication;

public class UserClaimsFactory(ECommerceDbContext dbContext) : IUserClaimsFactory
{
    public async Task<IReadOnlyList<Claim>> CreateRoleClaimsAsync(User user, CancellationToken cancellationToken)
    {
        var roleNames = await dbContext.UserRoles
           .Where(ur => ur.UserId == user.Id)
           .Select(ur => ur.Role.Name)
           .ToListAsync(cancellationToken);

        return roleNames.Select(name => new Claim(ClaimTypes.Role, name)).ToList();
    }
}
