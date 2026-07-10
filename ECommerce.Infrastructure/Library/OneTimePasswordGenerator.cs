using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Library.Constants;
using OtpNet;

namespace ECommerce.Infrastructure.Library;

public class OneTimePasswordGenerator : IOneTimePasswordGenerator
{ 
    public string GenerateSecret()
    {
        byte[] key = KeyGeneration.GenerateRandomKey(Constants.OneTimePasswordConstants.SecretSizeBytes);
        return Base32Encoding.ToString(key);
    }

    public string GetCurrentCode(string base32Secret)
    {
        var totp = new Totp(Base32Encoding.ToBytes(base32Secret), step: OneTimePasswordConstants.StepSeconds);
        return totp.ComputeTotp();
    }

    public int GetRemainingSeconds(string base32Secret)
    {
        var oneTimePassword = new Totp(Base32Encoding.ToBytes(base32Secret), step: OneTimePasswordConstants.StepSeconds);
        return oneTimePassword.RemainingSeconds();
    }

    public bool VerifyCode(string base32Secret, string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || userCode.Length != 6)
        {
            return false;
        }

        var oneTimePassword = new Totp(Base32Encoding.ToBytes(base32Secret), step: OneTimePasswordConstants.StepSeconds);
        return oneTimePassword.VerifyTotp(
            userCode,
            out _,
            new VerificationWindow(previous: OneTimePasswordConstants.VerifyWindowSteps, future: OneTimePasswordConstants.VerifyWindowSteps)
        );
    }
}
