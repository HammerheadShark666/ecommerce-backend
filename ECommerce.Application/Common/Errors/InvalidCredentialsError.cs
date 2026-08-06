using FluentResults;

namespace ECommerce.Application.Common.Errors;

public sealed class InvalidCredentialsError : Error
{
    public InvalidCredentialsError()
        : base("The provided credentials are invalid.")
    {
    }
}
