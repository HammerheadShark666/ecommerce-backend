
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Infrastructure.Extensions;

public static class KeyVaultExtension
{
    public static WebApplicationBuilder AddKeyVaultExtension(this WebApplicationBuilder builder)
    {   
        if (builder.Environment.IsProduction())
        {
            string keyVaultUri = GetKeyVaultUri(builder);
            var credential = new DefaultAzureCredential();

            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                credential);
        }

        return builder;
    }

    private static string GetKeyVaultUri(WebApplicationBuilder builder)
    {
        string environment = builder.Environment.EnvironmentName;

        string? keyVaultUri = environment switch
        {
            "Development" => builder.Configuration["KeyVault:DevelopmentVaultUri"],
            "Production" => builder.Configuration["KeyVault:ProductionVaultUri"],
            _ => null
        } ?? throw new InvalidOperationException($"Key Vault configuration is missing. Vault URI not found.");

        return keyVaultUri;
    }
}
