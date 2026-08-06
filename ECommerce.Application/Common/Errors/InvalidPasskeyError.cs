using FluentResults;

namespace ECommerce.Application.Common.Errors;

public sealed class InvalidPasskeyError : Error
{
    public InvalidPasskeyError()
        : base("Invalid or expired code. Please try again.")
    {
    }
}
