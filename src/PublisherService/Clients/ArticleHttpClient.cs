using PublisherService.Contracts;

namespace PublisherService.Clients;

public class ArticleHttpClient(HttpClient httpClient) : IArticleClient
{
    public async Task<PublishArticleResponse> CreateArticleAsync(CreateArticleRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("/articles", request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PublishArticleResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException("No response from ArticleService.");
    }
}