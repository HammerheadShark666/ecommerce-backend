namespace ECommerce.Core.Exceptions;

public class DuplicateEmailException(string email) : DomainException($"User with email '{email}' already exists.")
{
}
