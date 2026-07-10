namespace ECommerce.Application.Exceptions;

public class InvalidTwoFactorStateException(string message) : DomainException(message)
{
}
