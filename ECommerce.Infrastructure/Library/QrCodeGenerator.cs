using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Library.Constants;
using QRCoder;

namespace ECommerce.Infrastructure.Library;

public class QrCodeGenerator : IQrCodeGenerator
{
    public string BuildOneTimePasswordAuthUri(string issuer, string accountName, string base32Secret)
    {
        string encodedIssuer = Uri.EscapeDataString(issuer);
        string encodedAccount = Uri.EscapeDataString(accountName);

        return $"otpauth://totp/{encodedIssuer}:{encodedAccount}" +
               $"?secret={base32Secret}" +
               $"&issuer={encodedIssuer}" +
               $"&algorithm={OneTimePasswordConstants.Algorithm}" +
               $"&digits={OneTimePasswordConstants.Digits}" +
               $"&period={OneTimePasswordConstants.Period}";
    }

    public string GenerateQrCodeBase64(string oneTimePasswordAuthUri, int pixelsPerModule = 10)
    {
        using var qrGenerator = new QRCodeGenerator();
        using QRCodeData qrData = qrGenerator.CreateQrCode(oneTimePasswordAuthUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);

        return Convert.ToBase64String(qrCode.GetGraphic(pixelsPerModule));
    }
}
