using System.Security.Claims;
using ECommerce.Domain.Entities.User;

namespace ECommerce.Application.Abstractions.Authentication;

public interface IUserClaimsFactory
{
    Task<IReadOnlyList<Claim>> CreateRoleClaimsAsync(User user, CancellationToken ct);
}
