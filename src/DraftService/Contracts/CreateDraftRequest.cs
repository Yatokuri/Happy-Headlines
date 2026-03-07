namespace DraftService.Contracts;

public class CreateDraftRequest
{
    public required string PublisherId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}