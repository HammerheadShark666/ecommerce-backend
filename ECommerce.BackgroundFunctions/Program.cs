using ECommerce.BackgroundFunctions.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ECommerce.Infrastructure.Extensions;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAzureCredentials();
builder.Services.AddOptions(builder.Configuration);
builder.Services.AddDependencyInjection();

builder.Build().Run(); 
