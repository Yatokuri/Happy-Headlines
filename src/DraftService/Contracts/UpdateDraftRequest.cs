namespace DraftService.Contracts;

public class UpdateDraftRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
}