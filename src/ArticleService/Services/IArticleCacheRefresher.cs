namespace ArticleService.Services;

public interface IArticleCacheRefresher
{
    Task RefreshLatest14DaysAsync(CancellationToken cancellationToken);
}