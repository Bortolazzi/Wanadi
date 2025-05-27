using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wanadi.Common.Attributes;
using Wanadi.Common.Contracts.Configurations;

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

    public static IServiceCollection AddHttpClientConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        var cookieContainer = new CookieContainer();

        services.AddSingleton(cookieContainer);
        services.AddHttpClient();

        var httpClientConfigurations = configuration.GetSection("HttpClientConfigurations").Get<List<HttpClientConfiguration>>();
        if (httpClientConfigurations is not { Count: > 0 })
            return services;

        if (httpClientConfigurations.GroupBy(t => t.Name).Any(t => t.Count() > 1))
            throw new Exception("It is not possible to add more than one httpclient with the same name.");

        foreach (var httpClientConfig in httpClientConfigurations)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = httpClientConfig.AllowAutoRedirect,
                UseCookies = httpClientConfig.UseCookies
            };

            if (httpClientConfig.UseCookies)
                httpClientHandler.CookieContainer = cookieContainer;

            if (httpClientConfig.AllowByPassCertificateCheck)
                httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            if (httpClientConfig is { ProxyAddress: not null, ProxyPassword: not null, ProxyUser: not null })
            {
                var proxy = new WebProxy(httpClientConfig.ProxyAddress);
                proxy.Credentials = new NetworkCredential(httpClientConfig.ProxyUser, httpClientConfig.ProxyPassword);
                httpClientHandler.Proxy = proxy;
            }

            services.AddHttpClient(httpClientConfig.Name, client =>
            {
                client.Timeout = TimeSpan.FromSeconds(httpClientConfig.TimeoutSeconds.GetValueOrDefault(60));
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);
        }

        return services;
    }
}