namespace ECommerce.Application.Exceptions;

public abstract class DomainException(string message) : Exception(message)
{
}
