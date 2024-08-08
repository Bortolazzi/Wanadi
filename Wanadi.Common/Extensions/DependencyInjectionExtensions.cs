using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Wanadi.Common.Attributes;

namespace Wanadi.Common.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection ResolveDependenciesByAttributes(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return services.ResolveDependenciesByAttributes(assemblies);
    }

    public static IServiceCollection ResolveDependenciesByAttributes(this IServiceCollection services, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var attribute = type.GetAttribute<DependencyInjectionAttribute>();
                if (attribute is not null)
                {
                    if (attribute.InjectionType == InjectionType.Scoped)
                        services.AddScoped(type);
                    else if (attribute.InjectionType == InjectionType.Singleton)
                        services.AddSingleton(type);
                    else if (attribute.InjectionType == InjectionType.Transient)
                        services.AddTransient(type);
                }
            }
        }

        return services;
    }
}