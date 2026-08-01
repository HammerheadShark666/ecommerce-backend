using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Seed_Data;

namespace ECommerce.Api.Extensions;

public static class SeedDataExtension
{
    public static async Task<WebApplicationBuilder> AddSeedDataExtensionAsync(this WebApplicationBuilder builder, WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using IServiceScope scope = app.Services.CreateScope();
            ECommerceDbContext dbContext = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
            await DatabaseSeeder.SeedAsync(dbContext);
        }

        return builder;
    }
}
