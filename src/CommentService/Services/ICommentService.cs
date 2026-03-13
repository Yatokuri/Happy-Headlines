using CommentService.Contracts;

namespace CommentService.Services;

public interface ICommentService
{
    Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CommentResponse>> GetCommentByArticleIdAsync(string articleId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CommentWithSourceResponse>> GetCommentWithSourceByArticleIdAsync(string articleId, CancellationToken cancellationToken);
}