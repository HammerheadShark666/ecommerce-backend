using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Background_Jobs;

public class TokenCleanupJob : BackgroundService
{
    private readonly Services.Intefaces.ITokenCleanupService _tokenCleanupService;
    private readonly ILogger<TokenCleanupJob> _logger;

    public TokenCleanupJob(Services.Intefaces.ITokenCleanupService tokenCleanupService, ILogger<TokenCleanupJob> logger)
    {
        _tokenCleanupService = tokenCleanupService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Register a callback to capture a stack trace when the token is cancelled
        stoppingToken.Register(() =>
            _logger.LogWarning("stoppingToken cancelled. StackTrace:\n{stack}", new System.Diagnostics.StackTrace(true).ToString()));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupAsync(stoppingToken);

                await Task.Delay(
                    TimeSpan.FromHours(1),
                    stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("TokenCleanupJob stopped due to host cancellation.");
        }
    }

    private async Task CleanupAsync(CancellationToken stoppingToken) =>
        await _tokenCleanupService.CleanupAsync(stoppingToken);
}
