using ProfanityService.Contracts;

namespace ProfanityService.Services;

public interface IProfanityService
{
    Task<CheckProfanityResponse> CheckTextAsync(string text, CancellationToken cancellationToken);
    Task<ProfanityWordResponse> AddWordAsync(string word, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProfanityWordResponse>> GetWordsAsync(CancellationToken cancellationToken);
}