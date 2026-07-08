namespace ECommerce.Core.Exceptions;

public class InvalidTwoFactorStateException(string message) : DomainException(message)
{
}
