using ECommerce.Infrastructure.Library.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Extensions;

public static class AuthenticationPolicyExtension
{
    public static AuthorizationBuilder AddAuthenticationPolicy(this IServiceCollection services) =>
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNamesConstants.CanManageProducts, policy =>
                policy.RequireRole(AuthenticationConstants.RoleAdmin))
            .AddPolicy(PolicyNamesConstants.CanManageOrders, policy =>
                policy.RequireRole(AuthenticationConstants.RoleAdmin))
            .AddPolicy(PolicyNamesConstants.CanManageUsers, policy =>
                policy.RequireRole(AuthenticationConstants.RoleAdmin));
}
