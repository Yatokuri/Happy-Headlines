using System.Diagnostics;
using NewsletterService.Clients;
using NewsletterService.Contracts;

namespace NewsletterService.Services;

public class NewsletterService(
    IArticleClient articleClient,
    ILogger<NewsletterService> logger)
    : INewsletterService
{
    private static readonly ActivitySource ActivitySource = new("NewsletterService");
    
    public async Task<SendNewsletterResponse> SendAsync(
        SendNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Send newsletter workflow");
        activity?.SetTag("newsletter.audience", request.Audience);
        activity?.SetTag("newsletter.max_articles", request.MaxArticles);

        IReadOnlyCollection<ArticleSummaryResponse> articles;
        
        using (var fetchActivity = ActivitySource.StartActivity("Fetch recent articles"))
        {
            articles = await articleClient.GetRecentArticlesAsync(request.MaxArticles, cancellationToken);
            fetchActivity?.SetTag("articles.count", articles.Count);
        }
        
        using var buildActivity = ActivitySource.StartActivity("Build newsletter payload");

        var articleIds = articles.Select(x => x.Id).ToList();
        
        // Does not actually send a newsletter - No email service has been connected
        buildActivity?.SetTag("articles.included", articleIds.Count);

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