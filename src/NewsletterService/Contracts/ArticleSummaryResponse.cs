namespace NewsletterService.Contracts;

public class ArticleSummaryResponse
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string PublisherId { get; set; }
    public required string ScopeType { get; set; }
    public required string ScopeValue { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}