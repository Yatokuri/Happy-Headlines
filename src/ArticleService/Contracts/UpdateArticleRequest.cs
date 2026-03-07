namespace ArticleService.Contracts;

public class UpdateArticleRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
}