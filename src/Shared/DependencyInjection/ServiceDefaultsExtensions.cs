using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Observability;

namespace Shared.DependencyInjection;

public static class ServiceDefaultsExtensions
{
    public static IServiceCollection AddServiceDefaults(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string serviceName)
    {
        services.AddHappyHeadlinesObservability(configuration, environment, serviceName);

        return services;
    }
}