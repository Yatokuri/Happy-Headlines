namespace NewsletterService.Contracts;

public class SendNewsletterRequest
{
    public required string Audience { get; set; }
    public int MaxArticles { get; set; } = 5;
}