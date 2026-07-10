using ECommerce.Domain.Entities.User;

namespace ECommerce.Application.Abstractions;

public interface IJwtGenerator
{
    Task<string> GenerateTokenAsync(User user);
}
