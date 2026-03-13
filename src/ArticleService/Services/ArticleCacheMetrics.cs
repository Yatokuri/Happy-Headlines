using System.Diagnostics.Metrics;

namespace ArticleService.Services;

public static class ArticleCacheMetrics
{
    public static readonly Meter Meter = new("HappyHeadlines.ArticleService.Cache");

    public static readonly Counter<long> CacheRequests =
        Meter.CreateCounter<long>("article_cache_requests_total");

    public static readonly Counter<long> CacheHits =
        Meter.CreateCounter<long>("article_cache_hits_total");

    public static readonly Counter<long> CacheMisses =
        Meter.CreateCounter<long>("article_cache_misses_total");
}