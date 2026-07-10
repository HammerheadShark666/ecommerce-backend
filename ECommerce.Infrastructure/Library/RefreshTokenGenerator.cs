using System.Security.Cryptography;
using ECommerce.Application.Abstractions;

namespace ECommerce.Infrastructure.Library;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
