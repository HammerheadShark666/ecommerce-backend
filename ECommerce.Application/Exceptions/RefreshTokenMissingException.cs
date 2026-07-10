namespace ECommerce.Application.Exceptions;

public class RefreshTokenMissingException() : DomainException("Refresh token is missing on login.")
{
}
