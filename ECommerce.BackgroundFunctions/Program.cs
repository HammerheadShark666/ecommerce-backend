using ECommerce.Application.Abstractions.Email;
using ECommerce.Infrastructure.Configurations;
using ECommerce.Infrastructure.Email;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication(); 

builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.Section));

builder.Services.AddScoped<IEmailSender, AzureCommunicationEmailSender>();

builder.Build().Run();

//FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();


//IHost host = new HostBuilder()
//    .ConfigureFunctionsWorkerDefaults()
//    .ConfigureServices((context, services) =>
//    {
//        string? endpoint = context.Configuration["Email:Endpoint"];

//        Console.WriteLine($"EMAIL ENDPOINT: {endpoint}");

//        services.Configure<EmailOptions>(context.Configuration.GetSection("Email"));
//    })
//    .Build();


//builder.Services.AddScoped<IEmailSender, AzureCommunicationEmailSender>();

//builder.Build().Run();
