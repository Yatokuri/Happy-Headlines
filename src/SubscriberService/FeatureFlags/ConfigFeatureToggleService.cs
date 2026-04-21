namespace SubscriberService.FeatureFlags;

public class ConfigFeatureToggleService(IConfiguration configuration) : IFeatureToggleService
{
    public bool IsSubscriberServiceEnabled()
    {
        return configuration.GetValue<bool>("Features:SubscriberServiceEnabled", true);
    }
}