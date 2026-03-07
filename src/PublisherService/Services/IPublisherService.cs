using PublisherService.Contracts;

namespace PublisherService.Services;

public interface IPublisherService
{
    Task<PublishArticleResponse> PublishAsync(
        PublishArticleRequest request,
        CancellationToken cancellationToken);
}