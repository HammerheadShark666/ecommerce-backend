namespace ECommerce.Application.Abstractions;

public interface IHmacsha256Hasher
{
    string HashToken(string token, string purpose, string hashSecret);
}
