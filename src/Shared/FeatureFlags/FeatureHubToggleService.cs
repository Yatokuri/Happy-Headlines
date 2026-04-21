using FeatureHubSDK;
using Microsoft.Extensions.Logging;

namespace Shared.FeatureFlags;

public class FeatureHubToggleService : IFeatureToggleService
{
    private readonly IFeatureHubConfig _featureHubConfig;
    private readonly ILogger<FeatureHubToggleService> _logger;
    private readonly Lazy<Task<IClientContext>> _contextFactory;

    public FeatureHubToggleService(
        IFeatureHubConfig featureHubConfig,
        ILogger<FeatureHubToggleService> logger)
    {
        _featureHubConfig = featureHubConfig;
        _logger = logger;

        _contextFactory = new Lazy<Task<IClientContext>>(CreateContextAsync);
    }

    public async Task<bool> IsEnabledAsync(string flagKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = await _contextFactory.Value;
            context.IsEnabled(flagKey);
            return context.IsEnabled(flagKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate FeatureHub flag {FlagKey}. Returning disabled.", flagKey);
            return false;
        }
    }

    private async Task<IClientContext> CreateContextAsync()
    {
        var context = await _featureHubConfig
            .NewContext()
            .UserKey("subscriber-service")
            .Build();

        return context;
    }
}