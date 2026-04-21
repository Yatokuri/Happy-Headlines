using Shared.Contracts;
using StackExchange.Redis;

namespace NewsletterService.BackgroundServices;

public class SubscriberWelcomeConsumer(
    IServiceProvider serviceProvider,
    ILogger<SubscriberWelcomeConsumer> logger,
    IConnectionMultiplexer redis)
    : BackgroundService
{
    private const string QueueKey = "subscriber-queue";
    private readonly IDatabase _db = redis.GetDatabase();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var value = await _db.ListLeftPopAsync(QueueKey);

            if (!value.HasValue)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                var message = System.Text.Json.JsonSerializer.Deserialize<SubscriberCreatedMessage>(value!);
                if (message is null) continue;

                using var scope = serviceProvider.CreateScope();
                var loggerScoped = scope.ServiceProvider.GetRequiredService<ILogger<SubscriberWelcomeConsumer>>();

                loggerScoped.LogInformation("Sending welcome mail to subscriber {Email}", message.Email);

                // For now simulate email send here
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing subscriber queue message.");
            }
        }
    }
}