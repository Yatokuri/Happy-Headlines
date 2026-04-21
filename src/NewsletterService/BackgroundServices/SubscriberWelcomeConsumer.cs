using System.Diagnostics;
using System.Text.Json;
using Shared.Contracts;
using StackExchange.Redis;

namespace NewsletterService.BackgroundServices;

public class SubscriberWelcomeConsumer(
    ILogger<SubscriberWelcomeConsumer> logger,
    IConnectionMultiplexer redis)
    : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("NewsletterService");
    private const string QueueKey = "subscriber-queue";
    private readonly IDatabase _db = redis.GetDatabase();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Subscriber welcome consumer started.");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var value = await _db.ListLeftPopAsync(QueueKey);
                if (!value.HasValue)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                using var activity = ActivitySource.StartActivity("Consume subscriber created event");

                var message = JsonSerializer.Deserialize<SubscriberCreatedMessage>(value!);

                if (message is null) continue;

                activity?.SetTag("subscriber.email", message.Email);

                using var sendActivity = ActivitySource.StartActivity("Send Welcome mail");
                sendActivity?.SetTag("subscriber.email", message.Email);

                logger.LogInformation("Sending welcome mail to new subscriber {Email}", message.Email);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing subscriber queue message.");
                await Task.Delay(1000, stoppingToken);
            }
        }
        logger.LogInformation("Subscriber welcome consumer stopped.");
    }
}