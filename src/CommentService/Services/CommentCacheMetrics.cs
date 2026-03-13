using System.Diagnostics.Metrics;

namespace CommentService.Services;

public static class CommentCacheMetrics
{
    public static readonly Meter Meter = new("HappyHeadlines.CommentService.Cache");

    public static readonly Counter<long> CacheRequests =
        Meter.CreateCounter<long>("comment_cache_requests_total");

    public static readonly Counter<long> CacheHits =
        Meter.CreateCounter<long>("comment_cache_hits_total");

    public static readonly Counter<long> CacheMisses =
        Meter.CreateCounter<long>("comment_cache_misses_total");

    public static readonly Counter<long> CacheEvictions =
        Meter.CreateCounter<long>("comment_cache_evictions_total");
}