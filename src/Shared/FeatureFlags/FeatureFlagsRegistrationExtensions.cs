using FeatureHubSDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.FeatureFlags;

public static class FeatureFlagRegistrationExtensions
{
    public static IServiceCollection AddHappyHeadlinesFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var host = configuration["FeatureHub:Host"];
        var apiKey = configuration["FeatureHub:ApiKey"];

        if (!string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(apiKey))
        {
            IFeatureHubConfig config = new EdgeFeatureHubConfig(host, apiKey);

            services.AddSingleton(config);

            config.Init();

            services.AddSingleton<IFeatureToggleService, FeatureHubToggleService>();
        }
        else
        {
            services.AddSingleton<IFeatureToggleService, ConfigFeatureToggleService>();
        }

        return services;
    }
}