namespace Shared.FeatureFlags;

public interface IFeatureToggleService
{
    Task<bool> IsEnabledAsync(string flagKey, CancellationToken cancellationToken = default);
}