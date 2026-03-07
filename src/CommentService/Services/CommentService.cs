using CommentService.Clients;
using CommentService.Contracts;
using CommentService.Data;
using CommentService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Services;

public class CommentService(
    CommentDbContext dbContext,
    IProfanityClient profanityClient,
    ILogger<CommentService> logger)
    : ICommentService
{
    public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken)
    {
        ProfanityCheckResponse profanityResult;

        try
        {
            profanityResult = await profanityClient.CheckTextAsync(request.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call ProfanityService while creating comment.");

            // Fallback behavior when ProfanityService is unavailable
            throw new InvalidOperationException("Profanity validation is currently unavailable. Please try again later.");
        }

        if (profanityResult.ContainsProfanity)
        {
            throw new ArgumentException(
                $"Comment contains profanity: {string.Join(", ", profanityResult.MatchedWords)}");
        }

        var entity = new Comment
        {
            Id = Guid.NewGuid(),
            ArticleId = request.ArticleId,
            AuthorName = request.AuthorName,
            Content = request.Content,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Comments.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CommentResponse
        {
            Id = entity.Id,
            ArticleId = entity.ArticleId,
            AuthorName = entity.AuthorName,
            Content = entity.Content,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }

    public async Task<IReadOnlyCollection<CommentResponse>> GetCommentByArticleIdAsync(string articleId, CancellationToken cancellationToken)
    {
        return await dbContext.Comments
            .Where(x => x.ArticleId == articleId)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => new CommentResponse
            {
                Id = x.Id,
                ArticleId = x.ArticleId,
                AuthorName = x.AuthorName,
                Content = x.Content,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}