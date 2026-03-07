using PublisherService.Clients;
using PublisherService.Contracts;

namespace PublisherService.Services;

public class PublisherService(
    IProfanityClient profanityClient,
    IArticleClient articleClient,
    ILogger<PublisherService> logger)
    : IPublisherService
{
    public async Task<PublishArticleResponse> PublishAsync(
        PublishArticleRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting publish workflow. PublisherId: {PublisherId}, ScopeType: {ScopeType}, ScopeValue: {ScopeValue}",
            request.PublisherId,
            request.ScopeType,
            request.ScopeValue);

        var profanityResult = await profanityClient.CheckTextAsync(request.Content, cancellationToken);

        if (profanityResult.ContainsProfanity)
        {
            logger.LogWarning(
                "Article rejected due to profanity. PublisherId: {PublisherId}, MatchedWords: {MatchedWords}",
                request.PublisherId,
                string.Join(", ", profanityResult.MatchedWords));

            throw new ArgumentException(
                $"Article contains profanity: {string.Join(", ", profanityResult.MatchedWords)}");
        }

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

        logger.LogInformation(
            "Article published successfully. ArticleId: {ArticleId}, PublisherId: {PublisherId}",
            article.Id,
            request.PublisherId);

        return article;
    }
}