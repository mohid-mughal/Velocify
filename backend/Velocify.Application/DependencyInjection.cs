using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Velocify.Application.Behaviors;

namespace Velocify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper with all profiles from this assembly
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register MediatR with pipeline behaviors
        // Pipeline behaviors execute in order: Validation → Logging → Performance
        // This order ensures validation happens first, then logging of valid requests, then performance measurement
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Register pipeline behaviors in execution order
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
