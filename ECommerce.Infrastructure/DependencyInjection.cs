using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        JwtOptions jwtOptions = configuration
          .GetSection(JwtOptions.Section)
          .Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt configuration section is missing");
         
        services.AddSqlServerExtension(configuration);
        services.AddInterfaceClassExtension();
        services.AddAuthenticationExtension();
        services.AddJwtExtension(jwtOptions);
        services.AddCors();
        services.AddMessaging(configuration); 
        services.AddAzureCredentials();

        return services;
    }
}
