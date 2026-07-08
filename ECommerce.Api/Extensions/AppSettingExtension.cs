namespace ECommerce.Api.Extensions;

public static class AppSettingExtension
{
    public static WebApplicationBuilder AddAppSettings(
        this WebApplicationBuilder builder)
    {
        builder.Configuration
        .AddJsonFile(
            $"appsettings.{builder.Environment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: true);

        if (builder.Environment.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        return builder;
    }
}
