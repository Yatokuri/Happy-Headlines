using ArticleService.Services;

namespace ArticleService.BackgroundServices;

public class ArticleCacheRefreshBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<ArticleCacheRefreshBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Article cache refresh background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var refresher = scope.ServiceProvider.GetRequiredService<IArticleCacheRefresher>();

                await refresher.RefreshLatest14DaysAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Article cache refresh failed.");
            }

            try
            {
                await Task.Delay(RefreshInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Article cache refresh background service stopped.");
    }
}