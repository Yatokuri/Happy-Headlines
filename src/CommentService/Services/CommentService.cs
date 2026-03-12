using System.Diagnostics;
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
    private static readonly ActivitySource ActivitySource = new("CommentService");
    
    public async Task<CommentResponse> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Create comment");
        activity?.SetTag("article.id", request.ArticleId);
        activity?.SetTag("author.name", request.AuthorName);
        
        ProfanityCheckResponse profanityResult;
        
        try
        {
            using var validateActivity = ActivitySource.StartActivity("Validate comment content");
            profanityResult = await profanityClient.CheckTextAsync(request.Content, cancellationToken);
            validateActivity?.SetTag("profanity.contains", profanityResult.ContainsProfanity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call ProfanityService while creating comment.");

            // Fallback behavior when ProfanityService is unavailable
            throw new InvalidOperationException("Profanity validation is currently unavailable. Please try again later.");
        }

        if (profanityResult.ContainsProfanity)
        {
            activity?.SetTag("comment.rejected", true);
            throw new ArgumentException(
                $"Comment contains profanity: {string.Join(", ", profanityResult.MatchedWords)}");
        }

        using var storeActivity = ActivitySource.StartActivity("Store comment");
        
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
        
        storeActivity?.SetTag("comment.id", entity.Id);

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
        using var activity = ActivitySource.StartActivity("Get comments for article");
        activity?.SetTag("article.id", articleId);
        
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