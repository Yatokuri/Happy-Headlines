using NewsletterService.Clients;
using NewsletterService.Contracts;

namespace NewsletterService.Services;

public class NewsletterService(
    IArticleClient articleClient,
    ILogger<NewsletterService> logger)
    : INewsletterService
{
    public async Task<SendNewsletterResponse> SendAsync(
        SendNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting newsletter send. Audience: {Audience}, MaxArticles: {MaxArticles}",
            request.Audience,
            request.MaxArticles);

        var articles = await articleClient.GetRecentArticlesAsync(
            request.MaxArticles,
            cancellationToken);

        var articleIds = articles.Select(x => x.Id).ToList();

        logger.LogInformation(
            "Newsletter content collected. Audience: {Audience}, ArticlesIncluded: {Count}",
            request.Audience,
            articleIds.Count);

        // Simulated send
        logger.LogInformation(
            "Newsletter sent. Audience: {Audience}, SentAtUtc: {SentAtUtc}",
            request.Audience,
            DateTime.UtcNow);

        return new SendNewsletterResponse
        {
            Audience = request.Audience,
            ArticlesIncluded = articleIds.Count,
            SentAtUtc = DateTime.UtcNow,
            ArticleIds = articleIds,
            Status = "Sent"
        };
    }
}