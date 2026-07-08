using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Core.Abstractions;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.Infrastructure.Extensions;

public static class SqlServerExtension
{
    public static void AddSqlServerExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ECommerceDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IECommerceDbContext>(sp =>
                sp.GetRequiredService<ECommerceDbContext>());
    }
}
