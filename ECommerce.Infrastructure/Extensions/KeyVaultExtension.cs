
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Infrastructure.Extensions;

public static class KeyVaultExtension
{
    public static IConfigurationManager AddKeyVaultExtension(
    this IConfigurationManager configuration,
    IHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            string keyVaultUri = GetKeyVaultUri(environment, configuration);

            configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }

        return configuration;
    } 
    
    private static string GetKeyVaultUri(IHostEnvironment environment, IConfiguration configuration)
    { 
        string env = environment.IsProduction() ? "Production" : "Development";

        string? keyVaultUri = env switch
        {
            "Development" => configuration["KeyVault:DevelopmentVaultUri"],
            "Production" => configuration["KeyVault:ProductionVaultUri"],
            _ => null
        } ?? throw new InvalidOperationException($"Key Vault configuration is missing. Vault URI not found.");

        return keyVaultUri;
    } 
}
