namespace ECommerce.Core.Exceptions;

public class RefreshTokenMissingException() : DomainException("Refresh token is missing on login.")
{
}
