using System.Text.Json;
using Shared.Contracts;
using StackExchange.Redis;

namespace SubscriberService.Queues;

public class RedisSubscriberQueuePublisher(IConnectionMultiplexer redis) : ISubscriberQueuePublisher
{
    private const string QueueKey = "subscriber-queue";
    private readonly IDatabase _db = redis.GetDatabase();

    public Task PublishSubscriberCreatedAsync(SubscriberCreatedMessage message)
    {
        var payload = JsonSerializer.Serialize(message);
        return _db.ListRightPushAsync(QueueKey, payload);
    }
}