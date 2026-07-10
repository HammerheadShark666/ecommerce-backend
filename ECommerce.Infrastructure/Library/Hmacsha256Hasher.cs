using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Abstractions;

namespace ECommerce.Infrastructure.Library;

public class Hmacsha256Hasher : IHmacsha256Hasher
{
    public string HashToken(string token, string purpose, string hashSecret)
    {
        byte[] hashSecretBytes = Convert.FromBase64String(hashSecret);

        using var hmac = new HMACSHA256(hashSecretBytes);

        return Convert.ToBase64String(
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes($"{purpose}:{token}")));
    }
}
