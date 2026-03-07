namespace CommentService.Models;

public class Comment
{
    public Guid Id { get; set; }
    public required string ArticleId { get; set; } 
    public required string AuthorName { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}