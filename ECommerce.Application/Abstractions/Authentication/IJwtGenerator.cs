using ECommerce.Domain.Entities.User;

namespace ECommerce.Application.Abstractions.Authentication;

public interface IJwtGenerator
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken);
}
