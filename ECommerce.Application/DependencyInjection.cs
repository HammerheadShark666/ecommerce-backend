using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Application.Extensions;
using ECommerce.Application.MediatR;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, IConfiguration configuration)
    {           
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>)); 
        services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

        services.AddApplicationSettings(configuration);

        return services;
    }
}
