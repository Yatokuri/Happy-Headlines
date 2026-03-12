using System.Diagnostics;
using System.Text.Json;
using ArticleService.Contracts;
using ArticleService.Models;
using ArticleService.Sharding;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ArticleService.Services;

public class ArticleService(
    IShardResolver shardResolver,
    IArticleDbContextFactory dbContextFactory,
    IArticleIdGenerator idGenerator,
    IConnectionMultiplexer redis)
    : IArticleService
{
    private static readonly ActivitySource ActivitySource = new("ArticleService");
    
    private readonly IDatabase _cache = redis.GetDatabase();

    private static readonly TimeSpan CacheWindow = TimeSpan.FromDays(14);

    private static string GetArticleCacheKey(string id) => $"article:{id}";

    public async Task<ArticleResponse> CreateAsync(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Create article");
        activity?.SetTag("article.scope_type", request.ScopeType);
        activity?.SetTag("article.scope_value", request.ScopeValue);

        string shard;
        
        using (var shardActivity = ActivitySource.StartActivity("Resolve article shard"))
        {
            shard = shardResolver.ResolveForCreate(request.ScopeType, request.ScopeValue);
            shardActivity?.SetTag("article.shard", shard);
        }
        
        var articleId = idGenerator.Generate(shard);

        await using var db = dbContextFactory.CreateDbContext(shard);

        var now = DateTime.UtcNow;

        var entity = new Article
        {
            Id = articleId,
            Title = request.Title,
            Content = request.Content,
            PublisherId = request.PublisherId,
            ScopeType = request.ScopeType,
            ScopeValue = request.ScopeValue,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Articles.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        
        activity?.SetTag("article.id", entity.Id);

        var result = Map(entity);

        await CacheArticleIfEligibleAsync(result);

        return result;
    }

    public async Task<IReadOnlyCollection<ArticleResponse>> GetRecentAsync(int limit, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Get recent articles");
        activity?.SetTag("articles.limit", limit);
        
        limit = Math.Clamp(limit, 1, 100);

        await using var db = dbContextFactory.CreateDbContext(ShardNames.Global);

        var articles = await db.Articles
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        activity?.SetTag("articles.count", articles.Count);

        return articles.Select(Map).ToList();
    }

    public async Task<ArticleResponse?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Get article by id");
        activity?.SetTag("article.id", id);
        
        var cacheKey = GetArticleCacheKey(id);

        var cached = await _cache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedArticle = JsonSerializer.Deserialize<ArticleResponse>(cached!);
            if (cachedArticle is not null)
            {
                return cachedArticle;
            }
        }

        var shard = shardResolver.ResolveFromArticleId(id);
        activity?.SetTag("article.shard", shard);

        await using var db = dbContextFactory.CreateDbContext(shard);

        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return null;

        var result = Map(entity);

        await CacheArticleIfEligibleAsync(result);

        return result;
    }

    public async Task<ArticleResponse?> UpdateAsync(string id, UpdateArticleRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Update article");
        activity?.SetTag("article.id", id);
        
        var shard = shardResolver.ResolveFromArticleId(id);
        activity?.SetTag("article.shard", shard);

        await using var db = dbContextFactory.CreateDbContext(shard);

        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return null;

        entity.Title = request.Title;
        entity.Content = request.Content;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        var result = Map(entity);

        await CacheArticleIfEligibleAsync(result);

        return result;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Delete article");
        activity?.SetTag("article.id", id);
        
        var shard = shardResolver.ResolveFromArticleId(id);
        activity?.SetTag("article.shard", shard);

        await using var db = dbContextFactory.CreateDbContext(shard);

        var entity = await db.Articles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
            return false;

        db.Articles.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        await _cache.KeyDeleteAsync(GetArticleCacheKey(id));

        return true;
    }

    private async Task CacheArticleIfEligibleAsync(ArticleResponse article)
    {
        var ttl = CalculateRemainingTtl(article.CreatedAtUtc);
        var cacheKey = GetArticleCacheKey(article.Id);

        if (ttl is null)
        {
            await _cache.KeyDeleteAsync(cacheKey);
            return;
        }

        var json = JsonSerializer.Serialize(article);

        await _cache.StringSetAsync(cacheKey, json);
        await _cache.KeyExpireAsync(cacheKey, ttl.Value);
    }

    private static TimeSpan? CalculateRemainingTtl(DateTime createdAtUtc)
    {
        var expiresAtUtc = createdAtUtc.Add(CacheWindow);
        var remaining = expiresAtUtc - DateTime.UtcNow;

        return remaining > TimeSpan.Zero ? remaining : null;
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