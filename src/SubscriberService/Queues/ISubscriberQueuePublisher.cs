using Shared.Contracts;

namespace SubscriberService.Queues;

public interface ISubscriberQueuePublisher
{
    Task PublishSubscriberCreatedAsync(SubscriberCreatedMessage message);
}