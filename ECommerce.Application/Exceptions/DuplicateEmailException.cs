namespace ECommerce.Application.Exceptions;

public class DuplicateEmailException(string email) : DomainException($"User with email '{email}' already exists.")
{
}
