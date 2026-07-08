namespace ECommerce.Core.Abstractions;

public interface IOneTimePasswordGenerator
{
    string GenerateSecret();

    string GetCurrentCode(string base32Secret);

    bool VerifyCode(string base32Secret, string userCode);
}
