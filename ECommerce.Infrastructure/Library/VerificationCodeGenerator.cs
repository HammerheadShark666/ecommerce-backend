using System.Security.Cryptography;
using ECommerce.Application.Abstractions;

namespace ECommerce.Infrastructure.Library;

public class VerificationCodeGenerator : IVerificationCodeGenerator
{
    public string Generate()
    {
        int value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }
}
