using ArticleService.Contracts;

namespace ArticleService.Services;

public interface IArticleService
{
    Task<ArticleResponse> CreateAsync(CreateArticleRequest request, CancellationToken cancellationToken);
    Task<ArticleResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<ArticleResponse?> UpdateAsync(string id, UpdateArticleRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ArticleResponse>> GetRecentAsync(int limit, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ArticleResponse>> GetLatestCachedWindowAsync(CancellationToken cancellationToken);
}