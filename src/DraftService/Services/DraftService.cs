using DraftService.Contracts;
using DraftService.Data;
using DraftService.Models;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Services;

public class DraftService(DraftDbContext dbContext, ILogger<DraftService> logger) : IDraftService
{
    public async Task<DraftResponse> CreateDraftAsync(CreateDraftRequest request, CancellationToken cancellationToken)
    {
        var entity = new Draft
        {
            Id = Guid.NewGuid(),
            PublisherId = request.PublisherId,
            Title = request.Title,
            Content = request.Content,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Drafts.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Draft created. DraftId: {DraftId}, PublisherId: {PublisherId}",
            entity.Id,
            entity.PublisherId);

        return Map(entity);
    }

    public async Task<DraftResponse?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Drafts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            logger.LogWarning("Draft not found. DraftId: {DraftId}", id);
            return null;
        }

        logger.LogInformation("Draft retrieved. DraftId: {DraftId}", id);

        return Map(entity);
    }

    public async Task<IReadOnlyCollection<DraftResponse>> GetDraftByPublisherIdAsync(
        string publisherId,
        CancellationToken cancellationToken)
    {
        var drafts = await dbContext.Drafts
            .Where(x => x.PublisherId == publisherId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new DraftResponse
            {
                Id = x.Id,
                PublisherId = x.PublisherId,
                Title = x.Title,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync(cancellationToken);

        logger.LogInformation(
            "Drafts retrieved for publisher. PublisherId: {PublisherId}, Count: {Count}",
            publisherId,
            drafts.Count);

        return drafts;
    }

    public async Task<DraftResponse?> UpdateDraftAsync(Guid id, UpdateDraftRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Drafts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            logger.LogWarning("Draft update failed. Draft not found. DraftId: {DraftId}", id);
            return null;
        }

        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Draft updated. DraftId: {DraftId}", id);

        return Map(entity);
    }

    public async Task<bool> DeleteDraftAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Drafts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            logger.LogWarning("Draft delete failed. Draft not found. DraftId: {DraftId}", id);
            return false;
        }

        dbContext.Drafts.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Draft deleted. DraftId: {DraftId}", id);

        return true;
    }

    private static DraftResponse Map(Draft entity)
    {
        return new DraftResponse
        {
            Id = entity.Id,
            PublisherId = entity.PublisherId,
            Title = entity.Title,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}