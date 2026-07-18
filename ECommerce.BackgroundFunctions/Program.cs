using ECommerce.Application.Extensions;
using ECommerce.BackgroundFunctions.Extensions;
using ECommerce.Infrastructure.Extensions;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddAzureCredentials();
builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddOptions(builder.Configuration);
builder.Services.AddSqlServerExtension(builder.Configuration);
builder.Services.AddDependencyInjection();

builder.Build().Run(); 
