using System.Diagnostics;
using System.Text.Json;
using CommentService.Clients;
using CommentService.Contracts;
using CommentService.Data;
using CommentService.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace CommentService.Services;

public class CommentService(
    CommentDbContext dbContext,
    IProfanityClient profanityClient,
    ILogger<CommentService> logger,
    IConnectionMultiplexer redis)
    : ICommentService
{
    private static readonly ActivitySource ActivitySource = new("CommentService");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase _cache = redis.GetDatabase();

    private const int CachedArticlesLimit = 30;
    private const string ArticlesLruKey = "comments:articles:lru";

    private static string GetArticleCommentsCacheKey(string articleId) => $"article:{articleId}:comments";

    public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Create comment");
        activity?.SetTag("article.id", request.ArticleId);
        activity?.SetTag("author.name", request.AuthorName);

        var profanityResult = await ValidateCommentAsync(request.Content, cancellationToken);

        if (profanityResult.ContainsProfanity)
        {
            activity?.SetTag("comment.rejected", true);
            throw new ArgumentException(
                $"Comment contains profanity: {string.Join(", ", profanityResult.MatchedWords)}");
        }

        var entity = new Comment
        {
            Id = Guid.NewGuid(),
            ArticleId = request.ArticleId,
            AuthorName = request.AuthorName,
            Content = request.Content,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Comments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = Map(entity);

        await RefreshArticleCommentsCacheAsync(request.ArticleId, cancellationToken);

        activity?.SetTag("comment.id", response.Id);

        return response;
    }

    public async Task<IReadOnlyCollection<CommentResponse>> GetCommentByArticleIdAsync(
        string articleId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Get comments for article");
        activity?.SetTag("article.id", articleId);

        var cacheKey = GetArticleCommentsCacheKey(articleId);
        var cached = await _cache.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            CommentCacheMetrics.CacheRequests.Add(1);
            CommentCacheMetrics.CacheHits.Add(1);
            
            var comments = JsonSerializer.Deserialize<List<CommentResponse>>(cached!, JsonOptions);
            if (comments is not null)
            {
                await TouchArticleAsync(articleId);

                activity?.SetTag("comments.source", "cache");
                activity?.SetTag("comments.count", comments.Count);

                return comments;
            }
        }
        CommentCacheMetrics.CacheRequests.Add(1);
        CommentCacheMetrics.CacheMisses.Add(1);

        var dbComments = await LoadCommentsFromDatabaseAsync(articleId, cancellationToken);

        await SetArticleCommentsCacheAsync(articleId, dbComments);
        await TouchArticleAsync(articleId);
        await EnforceArticleLruLimitAsync();

        activity?.SetTag("comments.source", "database");
        activity?.SetTag("comments.count", dbComments.Count);

        return dbComments;
    }
    
    public async Task<IReadOnlyCollection<CommentWithSourceResponse>> GetCommentWithSourceByArticleIdAsync(
        string articleId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Get comments with source for article");
        activity?.SetTag("article.id", articleId);

        var cacheKey = GetArticleCommentsCacheKey(articleId);
        var cached = await _cache.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            CommentCacheMetrics.CacheRequests.Add(1);
            CommentCacheMetrics.CacheHits.Add(1);
            
            var comments = JsonSerializer.Deserialize<List<CommentResponse>>(cached!, JsonOptions);

            if (comments is not null)
            {
                await TouchArticleAsync(articleId);

                activity?.SetTag("comments.source", "cache");
                activity?.SetTag("comments.count", comments.Count);

                return comments
                    .Select(c => new CommentWithSourceResponse
                    {
                        Id = c.Id,
                        ArticleId = c.ArticleId,
                        AuthorName = c.AuthorName,
                        Content = c.Content,
                        CreatedAtUtc = c.CreatedAtUtc,
                        Source = "cache"
                    })
                    .ToList();
            }
        }
        CommentCacheMetrics.CacheRequests.Add(1);
        CommentCacheMetrics.CacheMisses.Add(1);

        var dbComments = await LoadCommentsFromDatabaseAsync(articleId, cancellationToken);

        await SetArticleCommentsCacheAsync(articleId, dbComments);
        await TouchArticleAsync(articleId);
        await EnforceArticleLruLimitAsync();

        activity?.SetTag("comments.source", "database");
        activity?.SetTag("comments.count", dbComments.Count);

        return dbComments
            .Select(c => new CommentWithSourceResponse
            {
                Id = c.Id,
                ArticleId = c.ArticleId,
                AuthorName = c.AuthorName,
                Content = c.Content,
                CreatedAtUtc = c.CreatedAtUtc,
                Source = "database"
            })
            .ToList();
    }

    private async Task<ProfanityCheckResponse> ValidateCommentAsync(string content, CancellationToken cancellationToken)
    {
        try
        {
            using var activity = ActivitySource.StartActivity("Validate comment content");

            var result = await profanityClient.CheckTextAsync(content, cancellationToken);
            activity?.SetTag("profanity.contains", result.ContainsProfanity);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call ProfanityService while creating comment.");
            throw new InvalidOperationException("Profanity validation is currently unavailable. Please try again later.");
        }
    }

    private async Task<List<CommentResponse>> LoadCommentsFromDatabaseAsync(
        string articleId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Comments
            .Where(x => x.ArticleId == articleId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new CommentResponse
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                AuthorName = x.AuthorName,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }

    private async Task RefreshArticleCommentsCacheAsync(string articleId, CancellationToken cancellationToken)
    {
        var comments = await LoadCommentsFromDatabaseAsync(articleId, cancellationToken);

        await SetArticleCommentsCacheAsync(articleId, comments);
        await TouchArticleAsync(articleId);
        await EnforceArticleLruLimitAsync();
    }

    private Task SetArticleCommentsCacheAsync(string articleId, IReadOnlyCollection<CommentResponse> comments)
    {
        var cacheKey = GetArticleCommentsCacheKey(articleId);
        var payload = JsonSerializer.Serialize(comments, JsonOptions);

        return _cache.StringSetAsync(cacheKey, payload);
    }

    private Task<bool> TouchArticleAsync(string articleId)
    {
        var score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return _cache.SortedSetAddAsync(ArticlesLruKey, articleId, score);
    }

    private async Task EnforceArticleLruLimitAsync()
    {
        var count = await _cache.SortedSetLengthAsync(ArticlesLruKey);
        if (count <= CachedArticlesLimit)
            return;

        var numberToEvict = count - CachedArticlesLimit;

        var leastRecentlyUsedArticleIds = await _cache.SortedSetRangeByRankAsync(
            ArticlesLruKey,
            0,
            numberToEvict - 1,
            Order.Ascending);

        if (leastRecentlyUsedArticleIds.Length == 0)
            return;

        await _cache.SortedSetRemoveAsync(ArticlesLruKey, leastRecentlyUsedArticleIds);

        var keysToDelete = leastRecentlyUsedArticleIds
            .Select(x => (RedisKey)GetArticleCommentsCacheKey(x!))
            .ToArray();

        await _cache.KeyDeleteAsync(keysToDelete);
    }

    private static CommentResponse Map(Comment entity)
    {
        return new CommentResponse
        {
            Id = entity.Id,
            ArticleId = entity.ArticleId,
            AuthorName = entity.AuthorName,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}