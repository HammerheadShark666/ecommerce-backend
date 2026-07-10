using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library.Intefaces;

namespace ECommerce.IntegrationTests.Library;

public class TestApplicationFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.CaptureStartupErrors(true);
        builder.UseSetting("detailedErrors", "true");

        builder.ConfigureAppConfiguration((_, config) => config
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .AddEnvironmentVariables());

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Trace);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ECommerceDbContext>>();
            services.AddDbContext<ECommerceDbContext>(opts => opts.UseSqlServer(connectionString));
            services.AddScoped<IECommerceDbContext>(sp => sp.GetRequiredService<ECommerceDbContext>());
            services.AddScoped<IDatabaseHelper, DatabaseHelper>();

            services.RemoveAll<IOneTimePasswordGenerator>();
            services.AddSingleton<IOneTimePasswordGenerator, FakeOneTimePasswordGenerator>();
        });
    }
}
