using NewsletterService.Contracts;

namespace NewsletterService.Clients;

public interface IArticleClient
{
    Task<IReadOnlyCollection<ArticleSummaryResponse>> GetRecentArticlesAsync(
        int maxArticles,
        CancellationToken cancellationToken);
}