using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ECommerce.Infrastructure.Services.Intefaces;

namespace ECommerce.Infrastructure.Background_Jobs;

public sealed class TokenCleanupJob(
    IServiceScopeFactory scopeFactory, 
    TimeProvider timeProvider) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly TimeProvider _timeProvider = timeProvider;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);

            await Task.Delay(
                TimeSpan.FromHours(1),
                stoppingToken);
        }
    }

    private async Task CleanupAsync(
        CancellationToken cancellationToken)
    {
        using IServiceScope scope =
            _scopeFactory.CreateScope();

        ITokenCleanupService cleanupService =
            scope.ServiceProvider
                .GetRequiredService<ITokenCleanupService>();

        await cleanupService.CleanupAsync(cancellationToken);
    }
}
