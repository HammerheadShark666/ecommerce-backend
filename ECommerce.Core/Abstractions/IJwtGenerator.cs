using ECommerce.Domain.Entities.User;

namespace ECommerce.Core.Abstractions;

public interface IJwtGenerator
{
    Task<string> GenerateTokenAsync(User user);
}
