using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Extensions;

public static class AuthenticationExtension
{
    public static void AddJwtAuthentication(this IServiceCollection services) => services
          .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer();
}
