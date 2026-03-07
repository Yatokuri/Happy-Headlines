namespace NewsletterService.Contracts;

public class SendNewsletterResponse
{
    public required string Audience { get; set; }
    public int ArticlesIncluded { get; set; }
    public DateTime SentAtUtc { get; set; }
    public required List<string> ArticleIds { get; set; }
    public required string Status { get; set; }
}