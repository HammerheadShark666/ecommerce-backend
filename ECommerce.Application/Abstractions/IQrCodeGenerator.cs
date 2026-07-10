namespace ECommerce.Application.Abstractions;

public interface IQrCodeGenerator
{
    string BuildOneTimePasswordAuthUri(string issuer, string accountName, string base32Secret);

    string GenerateQrCodeBase64(string oneTimePasswordAuthUri, int pixelsPerModule = 10);
}
