namespace ECommerce.Core.Constants;

public static class OneTimePasswordConstants
{   
    public const string Algorithm = "SHA1";
    public const int Digits = 6;
    public const int Period = 30;

    public const int SecretSizeBytes = 20;
    public const int StepSeconds = 30;
    public const int VerifyWindowSteps = 1;
}
