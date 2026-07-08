namespace ECommerce.Infrastructure.Services.Intefaces;

public interface ITokenCleanupService
{
    Task CleanupAsync(CancellationToken cancellationToken);
}
