using eCommerce.ProductsService.Application.Abstractions.Messaging;
using eCommerce.ProductsService.Application.Behaviors;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace eCommerce.ProductsService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<HandlerExecutor>();
        services.AddScoped<IValidationService, ValidationService>();

        services.AddHandlersFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IDispatcher, Dispatcher>();

        return services;
    }

    // Método auxiliar para registrar todos los handlers de comandos y consultas
    private static void AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))));

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

            foreach (var handlerInterface in interfaces)
            {
                // Registra cada handler con su interfaz correspondiente
                services.AddScoped(handlerInterface, handlerType);
            }
        }
    }
}
