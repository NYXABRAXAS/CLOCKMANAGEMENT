using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using STLMS.Application.Common.Behaviors;
using STLMS.Application.Common.Mediator;

namespace STLMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<IAppMediator, AppMediator>();
        services.AddValidatorsFromAssembly(assembly);

        RegisterOpenGenericImplementations(services, assembly, typeof(IRequestHandler<,>));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }

    private static void RegisterOpenGenericImplementations(IServiceCollection services, Assembly assembly, Type openGenericInterface)
    {
        var matches =
            from type in assembly.GetTypes()
            where !type.IsAbstract && !type.IsInterface
            from @interface in type.GetInterfaces()
            where @interface.IsGenericType && @interface.GetGenericTypeDefinition() == openGenericInterface
            select new { Implementation = type, Service = @interface };

        foreach (var match in matches)
        {
            services.AddScoped(match.Service, match.Implementation);
        }
    }
}
