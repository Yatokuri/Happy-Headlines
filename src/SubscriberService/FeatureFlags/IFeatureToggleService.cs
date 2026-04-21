namespace SubscriberService.FeatureFlags;

public interface IFeatureToggleService
{
    bool IsSubscriberServiceEnabled();
}