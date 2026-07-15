using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Intercepters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Extensions;

public static class SqlServerExtension
{
    public static void AddSqlServerExtension(this IServiceCollection services, IConfiguration configuration)
    { 
        services.AddSingleton<AuditableEntityInterceptor>();

        services.AddDbContext<ECommerceDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        services.AddScoped<IECommerceDbContext>(sp =>
                sp.GetRequiredService<ECommerceDbContext>());
    }
}
