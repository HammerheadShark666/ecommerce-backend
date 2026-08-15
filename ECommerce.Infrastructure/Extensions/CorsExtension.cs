using ECommerce.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Extensions;

internal static class CorsExtension
{
    public static void BuildCorsPolicy(this IServiceCollection services, IOptions<UrlOptions> urlOptions) => services.AddCors(options => options.AddPolicy("ECommerceFrontendPolicy", policy => policy.WithOrigins(
                                                                                             urlOptions.Value.FrontEnd
                                                                                         )
                                                                                         .AllowAnyHeader()
                                                                                         .AllowAnyMethod()
                                                                                         .AllowCredentials()));
}
