using ECommerce.Core.Abstractions.Configuration;

namespace ECommerce.Application.Configuration;

public class HashSettings : IHashSettings
{
    public string Secret { get; init; } = string.Empty;
}
