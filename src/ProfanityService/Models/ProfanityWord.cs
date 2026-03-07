namespace ProfanityService.Models;

public class ProfanityWord
{
    public Guid Id { get; set; }
    public required string Word { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}