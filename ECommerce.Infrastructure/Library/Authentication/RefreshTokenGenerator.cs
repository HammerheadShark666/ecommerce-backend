using System.Security.Cryptography;
using ECommerce.Application.Abstractions.Authentication;

namespace ECommerce.Infrastructure.Library.Authentication;

public class RefreshTokenGenerator : IRefreshTokenGenerator
{
    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
