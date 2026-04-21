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
    public async Task<SubscriberResponse> SubscribeAsync(SubscribeRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await dbContext.Subscribers
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Subscriber already exists.");

        var entity = new Subscriber
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Subscribers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await queuePublisher.PublishSubscriberCreatedAsync(new SubscriberCreatedMessage
        {
            Email = entity.Email,
            CreatedAtUtc = entity.CreatedAtUtc
        });

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
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var entity = await dbContext.Subscribers
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (entity is null)
            return false;

        dbContext.Subscribers.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyCollection<SubscriberResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Subscribers
            .OrderBy(x => x.Email)
            .Select(x => new SubscriberResponse
            {
                Id = x.Id,
                Email = x.Email,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}