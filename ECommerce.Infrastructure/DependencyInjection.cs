using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Extensions;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSqlServer(configuration);
        services.AddInterfaceClasses();
        services.AddJwtAuthentication();
        services.AddAuthenticationPolicy();
        services.AddJwt(GetJwtOptions(configuration));
        services.BuildCorsPolicy(GetUrlOptions(configuration));
        services.AddMessaging(configuration); 
        services.AddAzureCredentials();

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", p => p.RequireRole(AuthenticationConstants.RoleAdmin));

        return services;
    }

    private static UrlOptions GetUrlOptions(IConfiguration configuration) => configuration
          .GetSection(UrlOptions.Section)
          .Get<UrlOptions>()
          ?? throw new InvalidOperationException("Url configuration section is missing");

    private static JwtOptions GetJwtOptions(IConfiguration configuration) => configuration
          .GetSection(JwtOptions.Section)
          .Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt configuration section is missing");
}
