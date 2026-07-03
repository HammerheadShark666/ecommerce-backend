using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Infrastructure.Persistence;

public class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(string[] args)
    {
        string environment =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile(
                $"appsettings.{environment}.json",
                optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString =
            configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder =
            new DbContextOptionsBuilder<ECommerceDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ECommerceDbContext(optionsBuilder.Options);
    }
}
