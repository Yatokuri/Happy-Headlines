using PublisherService.Contracts;

namespace PublisherService.Clients;

public interface IProfanityClient
{
    Task<ProfanityCheckResponse> CheckTextAsync(string text, CancellationToken cancellationToken);
}