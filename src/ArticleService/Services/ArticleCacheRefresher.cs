using System.Text.Json;
using ArticleService.Models;
using ArticleService.Sharding;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ArticleService.Services;

public class ArticleCacheRefresher(
    IArticleDbContextFactory dbContextFactory,
    IConnectionMultiplexer redis,
    ILogger<ArticleCacheRefresher> logger)
    : IArticleCacheRefresher
{
    private readonly IDatabase _cache = redis.GetDatabase();

    private const string CachedArticlesByCreatedKey = "articles:cached:by-created";
    private static readonly TimeSpan CacheWindow = TimeSpan.FromDays(14);

    private static string GetArticleCacheKey(string id) => $"article:{id}";

    public async Task RefreshLatest14DaysAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var fromUtc = now.AddDays(-14);

        logger.LogInformation("Refreshing article cache for articles from {FromUtc} to {NowUtc}", fromUtc, now);

        var totalCached = 0;

        foreach (var shard in ShardNames.All)
        {
            await using var db = dbContextFactory.CreateDbContext(shard);
            
            var articles = await db.Articles
                .Where(x => x.CreatedAtUtc >= fromUtc)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(cancellationToken);
            foreach (var entity in articles)
            {
                var article = Map(entity);

                var ttl = CalculateRemainingTtl(article.CreatedAtUtc);
                if (ttl is null)
                    continue;

                var cacheKey = GetArticleCacheKey(article.Id);
                var json = JsonSerializer.Serialize(article);
                var createdScore = new DateTimeOffset(article.CreatedAtUtc).ToUnixTimeSeconds();

                await _cache.StringSetAsync(cacheKey, json, ttl, When.Always);
                await _cache.SortedSetAddAsync(CachedArticlesByCreatedKey, article.Id, createdScore);
                
                totalCached++;
            }
        }

        await RemoveExpiredIndexEntriesAsync(now);

        logger.LogInformation("Article cache refresh completed. Cached {Count} recent global articles.", totalCached);
    }

    private async Task RemoveExpiredIndexEntriesAsync(DateTime now)
    {
        var cutoffScore = new DateTimeOffset(now.AddDays(-14)).ToUnixTimeSeconds();

        var expiredIds = await _cache.SortedSetRangeByScoreAsync(
            CachedArticlesByCreatedKey,
            double.NegativeInfinity,
            cutoffScore,
            Exclude.Stop);

        if (expiredIds.Length == 0)
            return;

        await _cache.SortedSetRemoveAsync(CachedArticlesByCreatedKey, expiredIds);

        var keysToDelete = expiredIds
            .Select(x => (RedisKey)GetArticleCacheKey(x!))
            .ToArray();

        if (keysToDelete.Length > 0)
            await _cache.KeyDeleteAsync(keysToDelete);
    }

    private static TimeSpan? CalculateRemainingTtl(DateTime createdAtUtc)
    {
        var expiresAtUtc = createdAtUtc.Add(CacheWindow);
        var remaining = expiresAtUtc - DateTime.UtcNow;

        return remaining > TimeSpan.Zero ? remaining : null;
    }

    private static Contracts.ArticleResponse Map(Article entity)
    {
        return new Contracts.ArticleResponse
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