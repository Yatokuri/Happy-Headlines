using NewsletterService.Contracts;

namespace NewsletterService.Clients;

public class ArticleHttpClient(HttpClient httpClient) : IArticleClient
{
    public async Task<IReadOnlyCollection<ArticleSummaryResponse>> GetRecentArticlesAsync(
        int maxArticles,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(
            $"/articles?limit={maxArticles}",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<List<ArticleSummaryResponse>>(
            cancellationToken: cancellationToken);

        return result ?? [];
    }
}