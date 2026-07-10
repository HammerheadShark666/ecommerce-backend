using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Extensions;

internal static class CorsExtension
{
    public static void BuildCorsPolicy(this IServiceCollection services) => services.AddCors(options => options.AddPolicy("WoldsHrFrontendPolicy", policy => policy.WithOrigins(
                                                                                             "http://localhost:3000",
                                                                                             "http://localhost:3001"
                                                                                         )
                                                                                         .AllowAnyHeader()
                                                                                         .AllowAnyMethod()
                                                                                         .AllowCredentials()));
}
