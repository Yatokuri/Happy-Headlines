namespace CommentService.Contracts;

public class CreateCommentRequest
{
    public required string ArticleId { get; set; }
    public required string AuthorName { get; set; }
    public required string Content { get; set; }
}