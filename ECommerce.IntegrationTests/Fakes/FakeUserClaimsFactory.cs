using System.Security.Claims;
using ECommerce.Application.Abstractions.Authentication;
using ECommerce.Domain.Entities.User;

namespace ECommerce.IntegrationTests.Fakes;

public class FakeUserClaimsFactory : IUserClaimsFactory
{
    private readonly IReadOnlyList<Claim> _claims;

    public FakeUserClaimsFactory(params string[] roles) =>
        _claims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

    public Task<IReadOnlyList<Claim>> CreateRoleClaimsAsync(User user, CancellationToken ct)
        => Task.FromResult(_claims);
}
