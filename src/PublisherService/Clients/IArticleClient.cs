using PublisherService.Contracts;

namespace PublisherService.Clients;

public interface IArticleClient
{
    Task<PublishArticleResponse> CreateArticleAsync(CreateArticleRequest request, CancellationToken cancellationToken);
}