namespace ECommerce.Application.Abstractions.Authentication;

public interface IOneTimePasswordGenerator
{
    string GenerateSecret();

    string GetCurrentCode(string base32Secret);

    bool VerifyCode(string base32Secret, string userCode);
}
