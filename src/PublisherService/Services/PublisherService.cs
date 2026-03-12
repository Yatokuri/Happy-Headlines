using System.Diagnostics;
using PublisherService.Clients;
using PublisherService.Contracts;

namespace PublisherService.Services;

public class PublisherService(
    IProfanityClient profanityClient,
    IArticleClient articleClient,
    ILogger<PublisherService> logger)
    : IPublisherService
{
    private static readonly ActivitySource ActivitySource = new("PublisherService");
    
    public async Task<PublishArticleResponse> PublishAsync(
        PublishArticleRequest request,
        CancellationToken cancellationToken)
    {
        using var publishActivity = ActivitySource.StartActivity("Publish article workflow");
        
        publishActivity?.SetTag("publisher.id", request.PublisherId);
        publishActivity?.SetTag("article.scope_type", request.ScopeType);
        publishActivity?.SetTag("article.scope_value", request.ScopeValue);
        
        using (var validateActivity = ActivitySource.StartActivity("Validate article content"))
        {
            var profanityResult = await profanityClient.CheckTextAsync(request.Content, cancellationToken);
            validateActivity?.SetTag("profanity.contains", profanityResult.ContainsProfanity);

            if (profanityResult.ContainsProfanity)
            {
                validateActivity?.SetTag("profanity.matched_words", string.Join(", ", profanityResult.MatchedWords));

                logger.LogWarning(
                    "Article rejected due to profanity. PublisherId: {PublisherId}, MatchedWords: {MatchedWords}",
                    request.PublisherId,
                    string.Join(", ", profanityResult.MatchedWords));

                throw new ArgumentException(
                    $"Article contains profanity: {string.Join(", ", profanityResult.MatchedWords)}");
            }
        }
        
        using var persistActivity = ActivitySource.StartActivity("Add published article to database");

        var article = await articleClient.CreateArticleAsync(
            new CreateArticleRequest
            {
                Title = request.Title,
                Content = request.Content,
                PublisherId = request.PublisherId,
                ScopeType = request.ScopeType,
                ScopeValue = request.ScopeValue
            },
            cancellationToken);

        persistActivity?.SetTag("article.id", article.Id);
        
        logger.LogInformation(
            "Article published successfully. ArticleId: {ArticleId}, PublisherId: {PublisherId}",
            article.Id,
            request.PublisherId);

        return article;
    }
}