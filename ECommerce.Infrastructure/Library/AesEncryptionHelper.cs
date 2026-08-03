using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Abstractions;

namespace ECommerce.Infrastructure.Library; 

public class AesEncryptionHelper : IAesEncryptionHelper
{
    public string Encrypt(string plainText, string encryptionKey)
    {
        ArgumentNullException.ThrowIfNull(plainText);

        var key = DeriveKey(encryptionKey);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV(); // Random IV per encryption

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext so we can extract it on decrypt
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText, string encryptionKey)
    {
        ArgumentNullException.ThrowIfNull(cipherText);

        var key = DeriveKey(encryptionKey);
        var fullBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;

        // Extract IV from the front of the payload
        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullBytes.Length - iv.Length];
        Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullBytes, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    // Derives a consistent 32-byte key from whatever string is in the env var
    private static byte[] DeriveKey(string rawKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(rawKey);
        return SHA256.HashData(keyBytes); // 256-bit key
    }
}
