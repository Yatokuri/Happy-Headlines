using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;
using SubscriberService.Contracts;
using SubscriberService.Data;
using SubscriberService.Models;
using SubscriberService.Queues;

namespace SubscriberService.Services;

public class SubscriberService(
    SubscriberDbContext dbContext,
    ISubscriberQueuePublisher queuePublisher,
    ILogger<SubscriberService> logger)
    : ISubscriberService
{
    private static readonly ActivitySource ActivitySource = new("SubscriberService");
    
    public async Task<SubscriberResponse> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Subscribe user");
        activity?.SetTag("subscriber.email", request.Email);
        
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await dbContext.Subscribers
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            activity?.SetTag("subscriber.exists", true);
            throw new InvalidOperationException("Subscriber already exists.");
        }
        
        var entity = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Subscribers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        using (var queueActivity = ActivitySource.StartActivity("Queue subscriber created event"))
        {
            await queuePublisher.PublishSubscriberCreatedAsync(new SubscriberCreatedMessage
            {
                Email = entity.Email,
                CreatedAtUtc = entity.CreatedAtUtc
            });

            queueActivity?.SetTag("subscriber.email", entity.Email);
        }

        logger.LogInformation("Subscriber created and queued. Email: {Email}", entity.Email);

        return new SubscriberResponse
        {
            Id = entity.Id,
            Email = entity.Email,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public async Task<bool> UnsubscribeAsync(string email, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Unsubscribe user");
        activity?.SetTag("subscriber.email", email);
        
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var entity = await dbContext.Subscribers
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (entity is null)
        {
            activity?.SetTag("subscriber.found", false);
            return false;
        }
        
        dbContext.Subscribers.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<SubscriberResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Get all subscribers");
        
        var subscribers = await dbContext.Subscribers
            .OrderBy(x => x.Email)
            .Select(x => new SubscriberResponse
            {
                Id = x.Id,
                Email = x.Email,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        activity?.SetTag("subscribers.count", subscribers.Count);

        return subscribers;
    }
}