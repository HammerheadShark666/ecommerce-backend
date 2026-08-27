using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Library.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Extensions;

internal static class CorsExtension
{
    public static IServiceCollection BuildCorsPolicy(this IServiceCollection services, UrlOptions urlOptions) => 
                            services.AddCors(options => options.AddPolicy(PolicyNamesConstants.ECommerceFrontendCorsPolicy, policy => policy
                                        .WithOrigins(urlOptions.FrontEnd, 
                                                     UrlConstants.LocalBaseUrl, 
                                                     UrlConstants.LocalDebugBaseUrl)
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials()));
}
