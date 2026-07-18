using System.Security.Claims;
using System.Threading.RateLimiting;
using ECommerce.Application.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
            {
                string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                string key = userId
                            ?? context.Connection.RemoteIpAddress?.ToString()
                            ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = userId != null ? 100 : 50,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.AddFixedWindowLimiter(RateLimiterPolicyConstants.Login, policy =>
            {
                policy.PermitLimit = 5;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter(RateLimiterPolicyConstants.Register, policy =>
            {
                policy.PermitLimit = 3;
                policy.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter(RateLimiterPolicyConstants.ForgottonPassword, policy =>
            {
                policy.PermitLimit = 3;
                policy.Window = TimeSpan.FromMinutes(1);
            });

            options.AddFixedWindowLimiter(RateLimiterPolicyConstants.RefreshToken, policy =>
            {
                policy.PermitLimit = 10;
                policy.Window = TimeSpan.FromMinutes(1);
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests.", token);
            };
        }); 
 
        return services;
    }
}
