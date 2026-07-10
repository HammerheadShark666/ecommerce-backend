namespace ECommerce.Application.Abstractions;

public interface IRefreshTokenGenerator
{
    string GenerateRefreshToken();
}
