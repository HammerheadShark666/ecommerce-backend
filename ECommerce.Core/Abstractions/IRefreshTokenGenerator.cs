namespace ECommerce.Core.Abstractions;

public interface IRefreshTokenGenerator
{
    string GenerateRefreshToken();
}
