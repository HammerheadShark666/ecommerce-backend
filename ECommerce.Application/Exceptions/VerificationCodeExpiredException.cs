namespace ECommerce.Application.Exceptions;

public sealed class VerificationCodeExpiredException : Exception
{
    public VerificationCodeExpiredException()
        : base("The verification code has expired.")
    {
    }
}
