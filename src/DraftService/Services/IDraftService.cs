using DraftService.Contracts;

namespace DraftService.Services;

public interface IDraftService
{
    Task<DraftResponse> CreateDraftAsync(CreateDraftRequest request, CancellationToken cancellationToken);
    Task<DraftResponse?> GetDraftByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DraftResponse>> GetDraftByPublisherIdAsync(string publisherId, CancellationToken cancellationToken);
    Task<DraftResponse?> UpdateDraftAsync(Guid id, UpdateDraftRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteDraftAsync(Guid id, CancellationToken cancellationToken);
}