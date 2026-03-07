using ArticleService.Contracts;
using ArticleService.Models;
using ArticleService.Sharding;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Services;

public class ArticleService(
    IShardResolver shardResolver,
    IArticleDbContextFactory dbContextFactory,
    IArticleIdGenerator idGenerator)
    : IArticleService
{
    public async Task<ArticleResponse> CreateAsync(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var shard = shardResolver.ResolveForCreate(request.ScopeType, request.ScopeValue);
        var articleId = idGenerator.Generate(shard);

        await using var db = dbContextFactory.CreateDbContext(shard);

        var entity = new Article
        {
            Id = articleId,
            Title = request.Title,
            Content = request.Content,
            PublisherId = request.PublisherId,
            ScopeType = request.ScopeType,
            ScopeValue = request.ScopeValue,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.Articles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<ArticleResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var shard = shardResolver.ResolveFromArticleId(id);

        await using var db = dbContextFactory.CreateDbContext(shard);
        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<ArticleResponse?> UpdateAsync(string id, UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        var shard = shardResolver.ResolveFromArticleId(id);

        await using var db = dbContextFactory.CreateDbContext(shard);
        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var shard = shardResolver.ResolveFromArticleId(id);

        await using var db = dbContextFactory.CreateDbContext(shard);
        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return false;

        db.Articles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ArticleResponse Map(Article entity)
    {
        return new ArticleResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Content = entity.Content,
            PublisherId = entity.PublisherId,
            ScopeType = entity.ScopeType,
            ScopeValue = entity.ScopeValue,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}