namespace ECommerce.Application.Abstractions;

public interface IAesEncryptionHelper
{
    string Encrypt(string plainText, string encryptionKey);
    string Decrypt(string cipherText, string encryptionKey);
}
