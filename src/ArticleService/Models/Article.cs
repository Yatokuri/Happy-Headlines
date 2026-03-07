namespace ArticleService.Models;

public class Article
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string PublisherId { get; set; } 

    // "CONTINENT" or "GLOBAL"
    public required string ScopeType { get; set; }

    // Europe, Asia, Global, etc.
    public required string ScopeValue { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}