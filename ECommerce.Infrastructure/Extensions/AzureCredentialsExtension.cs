using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Infrastructure.Extensions;

public static class AzureCredentialsExtension
{
    public static void AddAzureCredentials(this IServiceCollection services) => 
                                           services.AddSingleton<TokenCredential>(sp =>
                                           {
                                               IHostEnvironment env = sp.GetRequiredService<IHostEnvironment>();

                                               return env.IsDevelopment()
                                                   ? new DefaultAzureCredential(new DefaultAzureCredentialOptions
                                                   {
                                                       ExcludeManagedIdentityCredential = true
                                                   })
                                                   : new DefaultAzureCredential();
                                           });
}
