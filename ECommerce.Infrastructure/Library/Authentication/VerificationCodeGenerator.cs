using System.Security.Cryptography;
using ECommerce.Application.Abstractions.Authentication;

namespace ECommerce.Infrastructure.Library.Authentication;

public class VerificationCodeGenerator : IVerificationCodeGenerator
{
    public string Generate()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");
    }
}
