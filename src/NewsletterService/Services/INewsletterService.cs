using NewsletterService.Contracts;

namespace NewsletterService.Services;

public interface INewsletterService
{
    Task<SendNewsletterResponse> SendAsync(SendNewsletterRequest request, CancellationToken cancellationToken);
}