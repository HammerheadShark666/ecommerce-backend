using ECommerce.Application.Abstractions;

namespace ECommerce.IntegrationTests.Library;

public class FakeOneTimePasswordGenerator : IOneTimePasswordGenerator
{
    private readonly string _fixedCode = "123456";

    public string GenerateSecret() => "FAKESECRET";

    public string GetCurrentCode(string base32Secret) => _fixedCode;

    public bool VerifyCode(string base32Secret, string userCode) => userCode == _fixedCode;
}
