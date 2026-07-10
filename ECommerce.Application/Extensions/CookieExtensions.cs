using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Extensions;

public static class CookieExtensions
{
    public static void SetRefreshToken(
        this HttpResponse response,
        string refreshToken,
        int expirationDays) => response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(expirationDays)
            });
}
