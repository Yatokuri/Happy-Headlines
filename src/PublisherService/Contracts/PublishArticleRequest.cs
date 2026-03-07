namespace PublisherService.Contracts;

public class PublishArticleRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string PublisherId { get; set; }
    public required string ScopeType { get; set; }
    public required string ScopeValue { get; set; }
}