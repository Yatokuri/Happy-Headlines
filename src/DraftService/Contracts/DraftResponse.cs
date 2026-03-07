namespace DraftService.Contracts;

public class DraftResponse
{
    public Guid Id { get; set; }
    public required string PublisherId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}