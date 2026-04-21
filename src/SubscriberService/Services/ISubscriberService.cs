using Shared.Contracts;
using SubscriberService.Contracts;


namespace SubscriberService.Services;

public interface ISubscriberService
{
    Task<SubscriberResponse> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken);
    Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubscriberResponse>> GetAllAsync(CancellationToken cancellationToken);
}