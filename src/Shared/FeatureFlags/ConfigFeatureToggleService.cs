using Microsoft.Extensions.Configuration;

namespace Shared.FeatureFlags;

public class ConfigFeatureToggleService(IConfiguration configuration) : IFeatureToggleService
{
    public Task<bool> IsEnabledAsync(string flagKey, CancellationToken cancellationToken = default)
    {
        var enabled = configuration.GetValue<bool>($"Features:{flagKey}", false);
        return Task.FromResult(enabled);
    }
}